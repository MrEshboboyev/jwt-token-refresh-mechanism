namespace Domain.Events;

public sealed record RefreshTokenRevokedDomainEvent(
    Guid Id, 
    Guid UserId,
    string Token) : DomainEvent(Id);