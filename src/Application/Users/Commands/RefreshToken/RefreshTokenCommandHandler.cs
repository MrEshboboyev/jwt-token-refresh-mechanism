using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Users.Common;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Domain.Shared;

namespace Application.Users.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var token = request.RefreshToken;
        
        #region Validate the refresh token
        
        var refreshToken = await refreshTokenRepository.GetAsync(token, cancellationToken);
        if (refreshToken is null)
        {
            return Result.Failure<RefreshTokenResponse>
                (DomainErrors.RefreshToken.InvalidToken);
        }

        if (refreshToken.IsExpired)
        {
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.RefreshToken.ExpiredToken);
        }

        if (refreshToken.IsRevoked)
        {
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.RefreshToken.RevokedToken);
        }
        
        #endregion
        
        #region Get the user associated with the refresh token
        
        var user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<RefreshTokenResponse>(
                DomainErrors.User.NotFound(refreshToken.UserId));
        }
        
        #endregion

        #region Generate a new access token
        
        var accessToken = jwtProvider.Generate(user);

        #endregion
        
        #region Optionally, generate a new refresh token and revoke the old one
        
        var newRefreshToken = tokenService.CreateRefreshToken(
            user.Id,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(newRefreshToken.Value);
        user.RevokeRefreshToken(refreshToken.Token);
        
        #endregion

        #region Save changes
        
        userRepository.Update(user);
        await refreshTokenRepository.AddAsync(newRefreshToken.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        #endregion

        return Result.Success(new RefreshTokenResponse(accessToken, newRefreshToken.Value.Token));
    }
}