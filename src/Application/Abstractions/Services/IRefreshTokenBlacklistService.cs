using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IRefreshTokenBlacklistService
{
    /// <summary>
    /// Adds a refresh token to the blacklist
    /// </summary>
    /// <param name="refreshToken">The refresh token to blacklist</param>
    /// <returns>True if the token was successfully blacklisted, false otherwise</returns>
    Task<bool> BlacklistTokenAsync(RefreshToken refreshToken);
    
    /// <summary>
    /// Checks if a refresh token is blacklisted
    /// </summary>
    /// <param name="hashedToken">The hashed refresh token to check</param>
    /// <returns>True if the token is blacklisted, false otherwise</returns>
    Task<bool> IsTokenBlacklistedAsync(string hashedToken);
    
    /// <summary>
    /// Removes expired tokens from the blacklist
    /// </summary>
    /// <returns>The number of tokens removed</returns>
    Task<int> RemoveExpiredTokensAsync();
}
