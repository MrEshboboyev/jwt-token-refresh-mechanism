using Application.Abstractions.Logging;
using Serilog;

namespace Infrastructure.Logging;

public sealed class TokenLogger(
    ILogger logger
) : ITokenLogger
{
    public void LogTokenCreated(Guid userId, string tokenId, string ipAddress)
    {
        logger.Information(
            "Token created for user {UserId} with token ID {TokenId} from IP {IpAddress}",
            userId,
            tokenId,
            ipAddress);
    }

    public void LogTokenRefreshed(Guid userId, string oldTokenId, string newTokenId, string ipAddress)
    {
        logger.Information(
            "Token refreshed for user {UserId}: old token {OldTokenId} replaced with new token {NewTokenId} from IP {IpAddress}",
            userId,
            oldTokenId,
            newTokenId,
            ipAddress);
    }

    public void LogTokenRevoked(Guid userId, string tokenId, string ipAddress)
    {
        logger.Information(
            "Token revoked for user {UserId} with token ID {TokenId} from IP {IpAddress}",
            userId,
            tokenId,
            ipAddress);
    }

    public void LogTokenBlacklisted(Guid userId, string tokenId, string reason)
    {
        logger.Warning(
            "Token blacklisted for user {UserId} with token ID {TokenId}: {Reason}",
            userId,
            tokenId,
            reason);
    }

    public void LogSuspiciousActivity(Guid userId, string tokenId, string reason, string ipAddress)
    {
        logger.Warning(
            "Suspicious activity detected for user {UserId} with token ID {TokenId} from IP {IpAddress}: {Reason}",
            userId,
            tokenId,
            ipAddress,
            reason);
    }

    public void LogConcurrentSessionLimitExceeded(Guid userId, int sessionCount, int limit)
    {
        logger.Warning(
            "Concurrent session limit exceeded for user {UserId}: {SessionCount} sessions, limit is {Limit}",
            userId,
            sessionCount,
            limit);
    }
}
