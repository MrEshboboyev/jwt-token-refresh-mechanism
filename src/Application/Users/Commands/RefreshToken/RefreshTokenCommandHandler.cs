using Application.Abstractions.Authentication;
using Application.Abstractions.Logging;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.Users.Common;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Microsoft.Extensions.Options;

namespace Application.Users.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    IClientInfoService clientInfoService,
    IRefreshTokenBlacklistService refreshTokenBlacklistService,
    IOptions<TokenPolicyOptions> tokenPolicyOptions,
    ITokenLogger tokenLogger
) : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly TokenPolicyOptions _tokenPolicyOptions = tokenPolicyOptions.Value;

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var token = request.RefreshToken;
        
        #region Validate the refresh token
        
        var refreshToken = await refreshTokenRepository.GetAsync(token, cancellationToken);
        if (refreshToken is null)
        {
            tokenLogger.LogSuspiciousActivity(Guid.Empty, "unknown", "Invalid refresh token provided", clientInfoService.GetIpAddress());
            return Result.Failure<RefreshTokenResponse>
                (DomainErrors.RefreshToken.InvalidToken);
        }

        if (refreshToken.IsExpired)
        {
            tokenLogger.LogTokenRevoked(refreshToken.UserId, refreshToken.Id.ToString(), clientInfoService.GetIpAddress());
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.RefreshToken.ExpiredToken);
        }

        if (refreshToken.IsRevoked)
        {
            // Potential replay attack - add to blacklist immediately
            await refreshTokenBlacklistService.BlacklistTokenAsync(refreshToken);
            tokenLogger.LogSuspiciousActivity(refreshToken.UserId, refreshToken.Id.ToString(), "Attempt to use revoked token", clientInfoService.GetIpAddress());
            tokenLogger.LogTokenBlacklisted(refreshToken.UserId, refreshToken.Id.ToString(), "Revoked token reuse attempt");
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.RefreshToken.RevokedToken);
        }
        
        // Check if the refresh token is being used from the same IP and user agent
        var currentIpAddress = clientInfoService.GetIpAddress();
        var currentUserAgent = clientInfoService.GetUserAgent();
        
        if (refreshToken.IpAddress != currentIpAddress || refreshToken.UserAgent != currentUserAgent)
        {
            // Possible token theft - revoke all user tokens and blacklist this one
            var userFromToken = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
            if (userFromToken is not null)
            {
                // Revoke all refresh tokens for this user
                foreach (var userRefreshToken in userFromToken.RefreshTokens.Where(rt => !rt.IsRevoked))
                {
                    userFromToken.RevokeRefreshToken(userRefreshToken.HashedToken);
                    await refreshTokenBlacklistService.BlacklistTokenAsync(userRefreshToken);
                    tokenLogger.LogTokenRevoked(userFromToken.Id, userRefreshToken.Id.ToString(), currentIpAddress);
                    tokenLogger.LogTokenBlacklisted(userFromToken.Id, userRefreshToken.Id.ToString(), "Security breach - token theft suspected");
                }
                
                userRepository.Update(userFromToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            
            // Blacklist the current token as well
            await refreshTokenBlacklistService.BlacklistTokenAsync(refreshToken);
            tokenLogger.LogSuspiciousActivity(refreshToken.UserId, refreshToken.Id.ToString(), "Token used from different IP/UserAgent", currentIpAddress);
            tokenLogger.LogTokenBlacklisted(refreshToken.UserId, refreshToken.Id.ToString(), "Token theft suspected");
            
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.RefreshToken.InvalidToken);
        }
        
        #endregion
        
        #region Get the user associated with the refresh token
        
        var user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            tokenLogger.LogSuspiciousActivity(refreshToken.UserId, refreshToken.Id.ToString(), "User not found", currentIpAddress);
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.User.NotFound(refreshToken.UserId));
        }
        
        #endregion

        #region Generate a new access token
        
        var accessToken = jwtProvider.Generate(user);

        #endregion
        
        #region Implement refresh token rotation - revoke the old token and create a new one
        
        // Revoke the current refresh token (one-time use policy)
        user.RevokeRefreshToken(refreshToken.HashedToken);
        tokenLogger.LogTokenRevoked(user.Id, refreshToken.Id.ToString(), currentIpAddress);
        
        // Add the old token to the blacklist
        await refreshTokenBlacklistService.BlacklistTokenAsync(refreshToken);
        tokenLogger.LogTokenBlacklisted(user.Id, refreshToken.Id.ToString(), "Token rotated");
        
        // Create a new refresh token with sliding window policy
        var newRefreshTokenValue = Guid.NewGuid().ToString();
        Result<Domain.Entities.RefreshToken> newRefreshToken;
        
        if (_tokenPolicyOptions.EnableSlidingWindowExpiration)
        {
            newRefreshToken = tokenService.CreateRefreshTokenWithPolicy(
                user.Id,
                newRefreshTokenValue,
                currentIpAddress,
                currentUserAgent,
                refreshToken.CreatedAt);
        }
        else
        {
            newRefreshToken = tokenService.CreateRefreshToken(
                user.Id,
                newRefreshTokenValue,
                DateTime.UtcNow.AddDays(_tokenPolicyOptions.RefreshTokenLifetimeDays),
                currentIpAddress,
                currentUserAgent);
        }
        
        user.AddRefreshToken(newRefreshToken.Value);
        tokenLogger.LogTokenCreated(user.Id, newRefreshToken.Value.Id.ToString(), currentIpAddress);
        tokenLogger.LogTokenRefreshed(user.Id, refreshToken.Id.ToString(), newRefreshToken.Value.Id.ToString(), currentIpAddress);
        
        #endregion

        #region Save changes
        
        userRepository.Update(user);
        await refreshTokenRepository.AddAsync(newRefreshToken.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        #endregion

        return Result.Success(new RefreshTokenResponse(accessToken, newRefreshTokenValue));
    }
}