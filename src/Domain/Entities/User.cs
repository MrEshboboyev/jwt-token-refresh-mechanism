using Domain.Errors;
using Domain.Events;
using Domain.Primitives;
using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class User : AggregateRoot, IAuditableEntity
{
    #region Private fields

    private readonly List<RefreshToken> _refreshTokens = [];

    #endregion

    #region Constructors

    private User(
        Guid id,
        Email email,
        string passwordHash,
        FullName fullName) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
    }

    #endregion

    #region Properties

    public string PasswordHash { get; set; }
    public FullName FullName { get; set; }
    public Email Email { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    #endregion

    #region Factory Methods

    public static User Create(
        Guid id,
        Email email,
        string passwordHash,
        FullName fullName
    )
    {
        #region Create new User

        var user = new User(
            id,
            email,
            passwordHash,
            fullName);

        #endregion

        #region Domain Events

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(
            Guid.NewGuid(),
            user.Id));

        #endregion

        return user;
    }

    #endregion

    #region Own Methods

    public void ChangeName(FullName fullName)
    {
        #region Checking new values are equals old valus

        if (!FullName.Equals(fullName))
        {
            RaiseDomainEvent(new UserNameChangedDomainEvent(
                Guid.NewGuid(),
                Id));
        }

        #endregion

        #region Update fields

        FullName = fullName;

        #endregion
    }

    #endregion

    #region Refresh token related methods

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _refreshTokens.Add(refreshToken);
        
        RaiseDomainEvent(new RefreshTokenCreatedDomainEvent(
            Guid.NewGuid(), 
            Id,
            refreshToken.Token));
    }

    public Result RevokeRefreshToken(string token)
    {
        #region Checking token is valid (or found)

        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        if (refreshToken is null)
        {
            return Result.Failure(
                DomainErrors.RefreshToken.InvalidToken);
        }

        #endregion
        
        #region Checking is revoked

        if (refreshToken.IsRevoked)
        {
            return Result.Failure(
                DomainErrors.RefreshToken.RevokedToken);
        }
        
        #endregion

        #region Revoke refresh token
        
        refreshToken.Revoke();
        
        #endregion
        
        #region DomainEvents
        
        RaiseDomainEvent(new RefreshTokenRevokedDomainEvent(
            Guid.NewGuid(), 
            Id,
            refreshToken.Token));
        
        #endregion
        
        return Result.Success();
    }

    #endregion
}