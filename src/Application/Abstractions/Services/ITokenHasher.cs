namespace Application.Abstractions.Services;

public interface ITokenHasher
{
    string HashToken(string token);
    bool VerifyToken(string token, string hashedToken);
}
