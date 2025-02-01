using Domain.Entities;
using Domain.Shared;

namespace Domain.Services;

public interface ITokenService
{
    Result<RefreshToken> CreateRefreshToken(Guid userId, string token, DateTime expiresAt);
    Result RevokeRefreshToken(User user, string token);
}