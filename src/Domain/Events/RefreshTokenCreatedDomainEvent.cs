namespace Domain.Events;

public sealed record RefreshTokenCreatedDomainEvent(
    Guid Id,
    Guid UserId,
    string Token) : DomainEvent(Id);