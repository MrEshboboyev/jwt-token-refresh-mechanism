namespace Application.Users.Common;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);