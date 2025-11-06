namespace Application.Abstractions.Logging;

public interface ITokenLogger
{
    void LogTokenCreated(Guid userId, string tokenId, string ipAddress);
    void LogTokenRefreshed(Guid userId, string oldTokenId, string newTokenId, string ipAddress);
    void LogTokenRevoked(Guid userId, string tokenId, string ipAddress);
    void LogTokenBlacklisted(Guid userId, string tokenId, string reason);
    void LogSuspiciousActivity(Guid userId, string tokenId, string reason, string ipAddress);
    void LogConcurrentSessionLimitExceeded(Guid userId, int sessionCount, int limit);
    void LogTokenCreationError(Guid userId, string error, string ipAddress);
    void LogDatabaseError(Guid userId, string operation, string error);
}
