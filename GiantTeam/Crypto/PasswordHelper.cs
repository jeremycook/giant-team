using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace GiantTeam.Crypto
{
    public static class PasswordHelper
    {
        private static readonly object stubUser = new();
        private static readonly PasswordHasher<object> passwordHasher = new();

        /// <summary>
        /// Returns a hashed representation of the supplied <paramref name="password"/>. 
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static string HashPlaintext(string plaintext)
        {
            return passwordHasher.HashPassword(stubUser, plaintext);
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="plaintext"/> is valid.
        /// </summary>
        /// <param name="hashtext"></param>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static bool VerifyHashedPlaintext(string hashtext, string plaintext)
        {
            return passwordHasher.VerifyHashedPassword(stubUser, hashtext, plaintext) != PasswordVerificationResult.Failed;
        }

        /// <summary>
        /// Returns a new password with the requested <paramref name="bitsOfEntropy"/>.
        /// </summary>
        /// <param name="bitsOfEntropy">Must be at least 128.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GeneratePassword(int bitsOfEntropy = 128)
        {
            if (bitsOfEntropy < 128)
                throw new ArgumentOutOfRangeException(nameof(bitsOfEntropy), bitsOfEntropy, $"The {nameof(bitsOfEntropy)} must be greater than or equal 128.");

            int numberOfBytes = (int)Math.Ceiling(bitsOfEntropy / 8f);
            byte[] bytes = RandomNumberGenerator.GetBytes(numberOfBytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }
    }
}
