using System.Security.Cryptography;

namespace BalanceFlow.Infrastructure.Security;

/// <summary>
/// Cryptographic password hashing helper using PBKDF2 with SHA-256 and unique 128-bit salts.
/// Prevents timing attacks and credential leakage.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a password using PBKDF2.
    /// </summary>
    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password hash using fixed-time comparison.
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var testHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithm,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch
        {
            return false;
        }
    }
}
