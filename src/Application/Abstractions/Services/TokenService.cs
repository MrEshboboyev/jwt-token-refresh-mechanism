using Application.Abstractions.Authentication;
using Domain.Entities;
using Domain.Errors;
using Domain.Shared;
using Microsoft.Extensions.Options;

namespace Application.Abstractions.Services;

public class TokenService(
    ITokenHasher tokenHasher,
    IOptions<TokenPolicyOptions> tokenPolicyOptions
) : ITokenService
{
    private readonly TokenPolicyOptions _tokenPolicyOptions = tokenPolicyOptions.Value;

    public Result<RefreshToken> CreateRefreshToken(Guid userId, string token, DateTime expiresAt, string ipAddress, string userAgent)
    {
        var hashedToken = tokenHasher.HashToken(token);
        var refreshToken = RefreshToken.Create(userId, token, hashedToken, expiresAt, ipAddress, userAgent, DateTime.UtcNow);
        return Result.Success(refreshToken);
    }

    public Result<RefreshToken> CreateRefreshTokenWithPolicy(Guid userId, string token, string ipAddress, string userAgent, DateTime? originalCreatedAt = null)
    {
        // Calculate expiration based on policy
        var expiresAt = DateTime.UtcNow.AddDays(_tokenPolicyOptions.RefreshTokenLifetimeDays);
        
        // If sliding window is enabled and we have an original creation time
        if (_tokenPolicyOptions.EnableSlidingWindowExpiration && originalCreatedAt.HasValue)
        {
            var now = DateTime.UtcNow;
            var originalExpiration = originalCreatedAt.Value.AddDays(_tokenPolicyOptions.RefreshTokenLifetimeDays);
            
            // Check if we're within the sliding window period
            var slidingWindowCutoff = originalExpiration.AddDays(-_tokenPolicyOptions.SlidingWindowPeriodDays);
            
            if (now >= slidingWindowCutoff && now < originalExpiration)
            {
                // Extend the expiration, but not beyond the maximum lifetime
                var maxExpiration = originalCreatedAt.Value.AddDays(_tokenPolicyOptions.MaxRefreshTokenLifetimeDays);
                expiresAt = new[] { originalExpiration.AddDays(_tokenPolicyOptions.RefreshTokenLifetimeDays), maxExpiration, DateTime.UtcNow.AddDays(_tokenPolicyOptions.RefreshTokenLifetimeDays) }.Min();
            }
        }
        
        var hashedToken = tokenHasher.HashToken(token);
        var refreshToken = RefreshToken.Create(userId, token, hashedToken, expiresAt, ipAddress, userAgent, originalCreatedAt ?? DateTime.UtcNow);
        return Result.Success(refreshToken);
    }

    public Result RevokeRefreshToken(User user, string token)
    {
        // For revocation, we need to find by the plain token value, but we're storing hashed tokens
        // This method should not be used directly anymore since we're using hashed tokens
        return Result.Failure(DomainErrors.RefreshToken.InvalidToken);
    }
}
