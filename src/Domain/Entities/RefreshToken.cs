using Domain.Primitives;

namespace Domain.Entities;

public sealed class RefreshToken : Entity, IAuditableEntity
{
    #region Constructors
    
    private RefreshToken(
        Guid id,
        Guid userId,
        string hashedToken,
        DateTime expiresAt,
        string ipAddress,
        string userAgent,
        DateTime createdAt,
        DateTime? revokedAt = null) : base(id)
    {
        UserId = userId;
        HashedToken = hashedToken;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = createdAt;
        RevokedAt = revokedAt;
    }
    
    #endregion
    
    #region Properties

    public Guid UserId { get; private set; }
    public string HashedToken { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    
    #endregion
    
    #region Factory Methods

    public static RefreshToken Create(
        Guid userId,
        string token,
        string hashedToken,
        DateTime expiresAt,
        string ipAddress,
        string userAgent,
        DateTime? createdAt = null)
    {
        var creationTime = createdAt ?? DateTime.UtcNow;
        
        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            hashedToken,
            expiresAt,
            ipAddress,
            userAgent,
            creationTime);
    }
    
    #endregion
    
    #region Own Methods

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
    
    #region Boolean Methods

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
    
    #endregion
    
    #endregion
}
