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
        
        Domain.Entities.RefreshToken? refreshToken;
        try
        {
            refreshToken = await refreshTokenRepository.GetAsync(token, cancellationToken);
        }
        catch (Exception ex)
        {
            tokenLogger.LogDatabaseError(Guid.Empty, "Failed to retrieve refresh token", ex.Message);
            return Result.Failure<RefreshTokenResponse>(DomainErrors.General.DatabaseError);
        }
        
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
            try
            {
                await refreshTokenBlacklistService.BlacklistTokenAsync(refreshToken);
            }
            catch
            {
                // Log error if logging is available
            }
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
            Domain.Entities.User? userFromToken;
            try
            {
                userFromToken = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
            }
            catch (Exception ex)
            {
                tokenLogger.LogDatabaseError(refreshToken.UserId, "Failed to retrieve user for token theft check", ex.Message);
                return Result.Failure<RefreshTokenResponse>(DomainErrors.General.DatabaseError);
            }
            
            if (userFromToken is not null)
            {
                try
                {
                    // Revoke all refresh tokens for this user
                    foreach (var userRefreshToken in userFromToken.RefreshTokens.Where(rt => !rt.IsRevoked))
                    {
                        userFromToken.RevokeRefreshToken(userRefreshToken.HashedToken);
                        try
                        {
                            await refreshTokenBlacklistService.BlacklistTokenAsync(userRefreshToken);
                        }
                        catch
                        {
                            // Log error if logging is available
                        }
                        tokenLogger.LogTokenRevoked(userFromToken.Id, userRefreshToken.Id.ToString(), currentIpAddress);
                        tokenLogger.LogTokenBlacklisted(userFromToken.Id, userRefreshToken.Id.ToString(), "Security breach - token theft suspected");
                    }
                    
                    userRepository.Update(userFromToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    tokenLogger.LogDatabaseError(userFromToken.Id, "Failed to revoke tokens for security breach", ex.Message);
                    // Continue execution even if we can't revoke all tokens
                }
            }
            
            // Blacklist the current token as well
            try
            {
                await refreshTokenBlacklistService.BlacklistTokenAsync(refreshToken);
            }
            catch
            {
                // Log error if logging is available
            }
            tokenLogger.LogSuspiciousActivity(refreshToken.UserId, refreshToken.Id.ToString(), "Token used from different IP/UserAgent", currentIpAddress);
            tokenLogger.LogTokenBlacklisted(refreshToken.UserId, refreshToken.Id.ToString(), "Token theft suspected");
            
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.RefreshToken.InvalidToken);
        }
        
        #endregion
        
        #region Get the user associated with the refresh token
        
        Domain.Entities.User? user;
        try
        {
            user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            tokenLogger.LogDatabaseError(refreshToken.UserId, "Failed to retrieve user", ex.Message);
            return Result.Failure<RefreshTokenResponse>(DomainErrors.General.DatabaseError);
        }
        
        if (user is null)
        {
            tokenLogger.LogSuspiciousActivity(refreshToken.UserId, refreshToken.Id.ToString(), "User not found", currentIpAddress);
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.User.NotFound(refreshToken.UserId));
        }
        
        #endregion

        #region Generate a new access token
        
        string accessToken;
        try
        {
            accessToken = jwtProvider.Generate(user);
        }
        catch (Exception ex)
        {
            tokenLogger.LogTokenCreationError(user.Id, $"Failed to generate JWT: {ex.Message}", currentIpAddress);
            return Result.Failure<RefreshTokenResponse>(DomainErrors.General.Unexpected);
        }

        #endregion

        #region Implement refresh token rotation - revoke the old token and create a new one

        // Create a new refresh token with sliding window policy
        var newRefreshTokenValue = Guid.NewGuid().ToString();
        Result<Domain.Entities.RefreshToken> newRefreshToken;
        try
        {
            // Revoke the current refresh token (one-time use policy)
            var revokeRefreshTokenResult = user.RevokeRefreshToken(refreshToken.HashedToken);
            if (revokeRefreshTokenResult.IsFailure)
            {
                tokenLogger.LogSuspiciousActivity(user.Id, refreshToken.Id.ToString(), "Failed to revoke token", currentIpAddress);
                // Continue execution even if we can't revoke the token
            }
            tokenLogger.LogTokenRevoked(user.Id, refreshToken.Id.ToString(), currentIpAddress);
            
            // Add the old token to the blacklist
            try
            {
                await refreshTokenBlacklistService.BlacklistTokenAsync(refreshToken);
            }
            catch
            {
                // Log error if logging is available
            }
            tokenLogger.LogTokenBlacklisted(user.Id, refreshToken.Id.ToString(), "Token rotated");
            
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
            
            if (newRefreshToken.IsFailure)
            {
                tokenLogger.LogTokenCreationError(user.Id, "Failed to create new refresh token", currentIpAddress);
                return Result.Failure<RefreshTokenResponse>(newRefreshToken.Error);
            }
            
            user.AddRefreshToken(newRefreshToken.Value);
            tokenLogger.LogTokenCreated(user.Id, newRefreshToken.Value.Id.ToString(), currentIpAddress);
            tokenLogger.LogTokenRefreshed(user.Id, refreshToken.Id.ToString(), newRefreshToken.Value.Id.ToString(), currentIpAddress);
        }
        catch (Exception ex)
        {
            tokenLogger.LogTokenCreationError(user.Id, $"Failed during token rotation: {ex.Message}", currentIpAddress);
            return Result.Failure<RefreshTokenResponse>(DomainErrors.General.Unexpected);
        }
        
        #endregion

        #region Save changes
        
        try
        {
            userRepository.Update(user);
            await refreshTokenRepository.AddAsync(newRefreshToken.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            tokenLogger.LogDatabaseError(user.Id, "Failed to save token changes", ex.Message);
            return Result.Failure<RefreshTokenResponse>(DomainErrors.General.DatabaseError);
        }
        
        #endregion

        return Result.Success(new RefreshTokenResponse(accessToken, newRefreshTokenValue));
    }
}
