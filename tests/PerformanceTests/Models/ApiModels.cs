namespace PerformanceTests.Models;

// Request models
public record LoginRequest(string Email, string Password);

public record RegisterUserRequest(string Email, string Password, string FullName);

public record RefreshTokenRequest(string RefreshToken);

public record RevokeTokenRequest(string RefreshToken);

// Response models
public record LoginResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiration, DateTime RefreshTokenExpiration);

public record RefreshTokenResponse(string AccessToken, DateTime AccessTokenExpiration);

public record ErrorResponse(string Type, string Title, int Status, string Detail, string Instance);
