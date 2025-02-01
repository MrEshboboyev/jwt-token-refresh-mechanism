namespace Presentation.Contracts.Users;

public sealed record RevokeTokenRequest(
    string RefreshToken);