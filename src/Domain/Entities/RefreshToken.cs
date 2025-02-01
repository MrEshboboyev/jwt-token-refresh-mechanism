using Domain.Primitives;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class RefreshToken : Entity, IAuditableEntity
{
    #region Constructors
    
    private RefreshToken(
        Guid id,
        Guid userId,
        string token,
        DateTime expiresAt,
        DateTime? revokedAt = null)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
    }
    
    #endregion
    
    #region Properties

    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    
    #endregion
    
    #region Factory Methods

    public static RefreshToken Create(
        Guid userId,
        string token,
        DateTime expiresAt)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            token,
            expiresAt);
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
    
    #endregion
    
    #endregion
}