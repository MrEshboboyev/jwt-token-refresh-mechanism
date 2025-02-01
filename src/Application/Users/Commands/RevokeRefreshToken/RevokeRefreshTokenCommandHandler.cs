using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using MediatR;

namespace Application.Users.Commands.RevokeRefreshToken;

public sealed class RevokeRefreshTokenCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeRefreshTokenCommand>
{
    public async Task<Result> Handle(
        RevokeRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var token = request.RefreshToken;
        
        #region Get the refresh token
        
        var refreshToken = await userRepository.GetRefreshTokenAsync(token, cancellationToken);
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