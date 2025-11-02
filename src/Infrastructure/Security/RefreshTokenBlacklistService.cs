using Application.Abstractions.Services;
using Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Infrastructure.Security;

public sealed class RefreshTokenBlacklistService(
    IDistributedCache cache
) : IRefreshTokenBlacklistService
{
    public async Task<bool> BlacklistTokenAsync(RefreshToken refreshToken)
    {
        try
        {
            var blacklistEntry = new BlacklistEntry
            {
                UserId = refreshToken.UserId,
                ExpiresAt = refreshToken.ExpiresAt,
                BlacklistedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(blacklistEntry);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = refreshToken.ExpiresAt
            };

            await cache.SetStringAsync(refreshToken.HashedToken, json, options);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string hashedToken)
    {
        try
        {
            var json = await cache.GetStringAsync(hashedToken);
            return !string.IsNullOrEmpty(json);
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> RemoveExpiredTokensAsync()
    {
        // Distributed cache automatically expires entries, so this is a no-op
        // In a real implementation with a database, this would remove expired entries
        return 0;
    }

    private class BlacklistEntry
    {
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime BlacklistedAt { get; set; }
    }
}
