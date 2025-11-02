using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IConcurrentLoginService
{
    /// <summary>
    /// Checks if the user has exceeded the maximum number of concurrent sessions
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <param name="maxConcurrentSessions">The maximum number of allowed concurrent sessions</param>
    /// <returns>True if the user has exceeded the limit, false otherwise</returns>
    bool HasExceededConcurrentSessions(User user, int maxConcurrentSessions);
    
    /// <summary>
    /// Gets information about all active sessions for a user
    /// </summary>
    /// <param name="user">The user to get sessions for</param>
    /// <returns>A list of active session information</returns>
    IEnumerable<SessionInfo> GetActiveSessions(User user);
    
    /// <summary>
    /// Terminates the oldest sessions for a user to stay within the limit
    /// </summary>
    /// <param name="user">The user whose sessions to manage</param>
    /// <param name="maxConcurrentSessions">The maximum number of allowed concurrent sessions</param>
    /// <returns>The number of sessions terminated</returns>
    int TerminateOldestSessions(User user, int maxConcurrentSessions);
}

public class SessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
