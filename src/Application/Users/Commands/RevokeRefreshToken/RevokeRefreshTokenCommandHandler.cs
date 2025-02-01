using Application.Abstractions.Messaging;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;

namespace Application.Users.Commands.RevokeRefreshToken;

internal sealed class RevokeRefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeRefreshTokenCommand>
{
    public async Task<Result> Handle(
        RevokeRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var token = request.RefreshToken;
        
        #region Get the refresh token
        
        var refreshToken = await refreshTokenRepository.GetAsync(token, cancellationToken);
        if (refreshToken is null)
        {
            return Result.Failure(
                DomainErrors.RefreshToken.InvalidToken);
        }
        
        #endregion

        #region Get the user associated with the refresh token
        
        var user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(refreshToken.UserId));
        }
        
        #endregion

        #region Revoke the refresh token
        
        var revokeRefreshTokenResult = user.RevokeRefreshToken(refreshToken.Token);
        if (revokeRefreshTokenResult.IsFailure)
        {
            return Result.Failure(
                revokeRefreshTokenResult.Error);
        }
        
        #endregion

        #region Save changes
        
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        #endregion

        return Result.Success();
    }
}