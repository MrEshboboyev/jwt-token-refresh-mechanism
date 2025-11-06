using Application.Abstractions.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Infrastructure.Security;

public sealed class TokenHasher : ITokenHasher
{
    public string HashToken(string token)
    {
        // Generate a random salt
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the token with the salt using PBKDF2
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: token,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        // Return salt + hashed token
        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

    public bool VerifyToken(string token, string hashedToken)
    {
        try
        {
            // Handle null or empty inputs
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hashedToken))
                return false;

            // Split the stored hash to get salt and hash
            var parts = hashedToken.Split('.');
            if (parts.Length != 2)
                return false;

            // Safely convert from base64
            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(parts[0]);
            }
            catch
            {
                return false;
            }

            var hash = parts[1];

            // Hash the provided token with the stored salt
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: token,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Compare the hashes using constant time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(hashed),
                System.Text.Encoding.UTF8.GetBytes(hash));
        }
        catch
        {
            return false;
        }
    }
}
