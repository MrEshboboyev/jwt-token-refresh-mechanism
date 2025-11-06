using Application.Abstractions.Logging;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;

namespace Application.Users.Commands.RevokeRefreshToken;

internal sealed class RevokeRefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IClientInfoService clientInfoService,
    ITokenLogger tokenLogger
) : ICommandHandler<RevokeRefreshTokenCommand>
{
    public async Task<Result> Handle(
        RevokeRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var token = request.RefreshToken;
        
        #region Get the refresh token
        
        Domain.Entities.RefreshToken? refreshToken;
        try
        {
            refreshToken = await refreshTokenRepository.GetAsync(token, cancellationToken);
        }
        catch (Exception ex)
        {
            tokenLogger.LogDatabaseError(Guid.Empty, "Failed to retrieve refresh token for revocation", ex.Message);
            return Result.Failure(DomainErrors.General.DatabaseError);
        }
        
        if (refreshToken is null)
        {
            tokenLogger.LogSuspiciousActivity(Guid.Empty, "revoke", "Attempt to revoke non-existent token", clientInfoService.GetIpAddress());
            return Result.Failure(
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
            tokenLogger.LogDatabaseError(refreshToken.UserId, "Failed to retrieve user during revoke", ex.Message);
            return Result.Failure(DomainErrors.General.DatabaseError);
        }
        
        if (user is null)
        {
            tokenLogger.LogSuspiciousActivity(refreshToken.UserId, refreshToken.Id.ToString(), "User not found during revoke", clientInfoService.GetIpAddress());
            return Result.Failure(DomainErrors.User.NotFound(refreshToken.UserId));
        }
        
        #endregion

        #region Check if the refresh token is being used from the same IP and user agent
        
        var currentIpAddress = clientInfoService.GetIpAddress();
        var currentUserAgent = clientInfoService.GetUserAgent();
        
        if (refreshToken.IpAddress != currentIpAddress || refreshToken.UserAgent != currentUserAgent)
        {
            // Possible token theft - this might be an unauthorized revocation attempt
            tokenLogger.LogSuspiciousActivity(user.Id, refreshToken.Id.ToString(), "Token revocation attempt from different IP/UserAgent", currentIpAddress);
            return Result.Failure(
                DomainErrors.RefreshToken.InvalidToken);
        }
        
        #endregion

        #region Revoke the refresh token
        
        var revokeRefreshTokenResult = user.RevokeRefreshToken(refreshToken.HashedToken);
        if (revokeRefreshTokenResult.IsFailure)
        {
            tokenLogger.LogSuspiciousActivity(user.Id, refreshToken.Id.ToString(), "Failed to revoke token", currentIpAddress);
            return Result.Failure(
                revokeRefreshTokenResult.Error);
        }
        
        tokenLogger.LogTokenRevoked(user.Id, refreshToken.Id.ToString(), currentIpAddress);
        
        #endregion

        #region Save changes
        
        try
        {
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            tokenLogger.LogDatabaseError(user.Id, "Failed to save revocation changes", ex.Message);
            return Result.Failure(DomainErrors.General.DatabaseError);
        }
        
        #endregion

        return Result.Success();
    }
}
