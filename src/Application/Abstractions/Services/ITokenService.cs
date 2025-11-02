using Domain.Entities;
using Domain.Shared;

namespace Application.Abstractions.Services;

public interface ITokenService
{
    Result<RefreshToken> CreateRefreshToken(Guid userId, string token, DateTime expiresAt, string ipAddress, string userAgent);
    Result<RefreshToken> CreateRefreshTokenWithPolicy(Guid userId, string token, string ipAddress, string userAgent, DateTime? originalCreatedAt = null);
    Result RevokeRefreshToken(User user, string token);
}
