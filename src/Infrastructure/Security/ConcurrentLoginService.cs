using Application.Abstractions.Services;
using Domain.Entities;

namespace Infrastructure.Security;

public sealed class ConcurrentLoginService : IConcurrentLoginService
{
    public bool HasExceededConcurrentSessions(User user, int maxConcurrentSessions)
    {
        var activeSessions = user.RefreshTokens.Count(rt => rt.IsActive);
        return activeSessions > maxConcurrentSessions;
    }

    public IEnumerable<SessionInfo> GetActiveSessions(User user)
    {
        return user.RefreshTokens
            .Where(rt => rt.IsActive)
            .Select(rt => new SessionInfo
            {
                SessionId = rt.Id.ToString(),
                CreatedAt = rt.CreatedAt,
                LastUsedAt = rt.CreatedAt, // For now, we don't track last used separately
                IpAddress = rt.IpAddress,
                UserAgent = rt.UserAgent,
                IsActive = rt.IsActive
            })
            .OrderByDescending(s => s.CreatedAt);
    }

    public int TerminateOldestSessions(User user, int maxConcurrentSessions)
    {
        var activeTokens = user.RefreshTokens
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.CreatedAt)
            .ToList();

        if (activeTokens.Count <= maxConcurrentSessions)
            return 0;

        var tokensToRevoke = activeTokens.Take(activeTokens.Count - maxConcurrentSessions).ToList();
        
        foreach (var token in tokensToRevoke)
        {
            // Revoke the token
            user.RevokeRefreshToken(token.HashedToken);
        }

        return tokensToRevoke.Count;
    }
}
