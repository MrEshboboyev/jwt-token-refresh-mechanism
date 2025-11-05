using PerformanceTests.Models;

namespace PerformanceTests.Helpers;

public static class TestDataHelper
{
    private static readonly Random Random = new();
    
    public static RegisterUserRequest GenerateRandomUser()
    {
        var email = $"testuser{Random.Next(100000, 999999)}@example.com";
        var password = GenerateRandomPassword();
        var fullName = $"Test User {Random.Next(1000, 9999)}";
        
        return new RegisterUserRequest(email, password, fullName);
    }
    
    public static LoginRequest GenerateLoginRequest(string email, string password)
    {
        return new LoginRequest(email, password);
    }
    
    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var password = new StringBuilder();
        
        // Ensure password meets complexity requirements
        password.Append(chars[Random.Next(0, 26)]); // At least one uppercase
        password.Append(chars[Random.Next(26, 52)]); // At least one lowercase
        password.Append(chars[Random.Next(52, 62)]); // At least one digit
        password.Append(chars[Random.Next(62, 70)]); // At least one special character
        
        // Add remaining characters
        for (int i = 4; i < 12; i++)
        {
            password.Append(chars[Random.Next(chars.Length)]);
        }
        
        return password.ToString();
    }
}
