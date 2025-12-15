using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Extensions
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;       // 128-bit salt
        private const int HashSize = 32;       // 256-bit hash
        private const int Iterations = 310000; // Recommended iteration count (adjust as needed)

        // Hashes a password and returns a base64 string containing salt + hash + iterations
        public static string HashPassword(this string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            // Derive the hash
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Combine salt + hash + iteration count
            byte[] hashBytes = new byte[SaltSize + HashSize + sizeof(int)];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
            Array.Copy(BitConverter.GetBytes(Iterations), 0, hashBytes, SaltSize + HashSize, sizeof(int));

            // Return as Base64 string
            return Convert.ToBase64String(hashBytes);
        }

        // Verifies a password against the stored hash string
        public static bool VerifyPassword(this string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Extract salt
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Extract iteration count
            int iterations = BitConverter.ToInt32(hashBytes, SaltSize + HashSize);

            // Extract stored hash
            byte[] storedHashBytes = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHashBytes, 0, HashSize);

            // Hash the provided password with extracted salt and iteration count
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(HashSize);

            // Compare hashes in constant time
            return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHash);
        }
    }
}
