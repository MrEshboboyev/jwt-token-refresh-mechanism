namespace Application.Users.Common;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken);