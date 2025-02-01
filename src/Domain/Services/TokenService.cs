using Domain.Entities;
using Domain.Errors;
using Domain.Shared;

namespace Domain.Services;

public class TokenService : ITokenService
{
    public Result<RefreshToken> CreateRefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        var refreshToken = RefreshToken.Create(userId, token, expiresAt);
        return Result.Success(refreshToken);
    }

    public Result RevokeRefreshToken(User user, string token)
    {
        var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == token);
        if (refreshToken is null)
        {
            return Result.Failure(
                DomainErrors.RefreshToken.InvalidToken);
        }

        if (refreshToken.IsRevoked)
        {
            return Result.Failure(
                DomainErrors.RefreshToken.RevokedToken);
        }

        refreshToken.Revoke();
        return Result.Success();
    }
}