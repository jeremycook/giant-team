using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace WebApp.Services
{
    public class HashingService
    {
        private static readonly object stubUser = new();
        private static readonly PasswordHasher<object> passwordHasher = new();

        /// <summary>
        /// Returns a base64url encoded random <paramref name="numberOfBytes"/>.
        /// </summary>
        /// <returns></returns>
        public string RandomPassword(int numberOfBytes = 16)
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(numberOfBytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        public string HashPlaintext(string plaintext)
        {
            return passwordHasher.HashPassword(stubUser, plaintext);
        }

        public bool VerifyHashedPlaintext(string? hashtext, string plaintext)
        {
            return passwordHasher.VerifyHashedPassword(stubUser, hashtext, plaintext) != PasswordVerificationResult.Failed;
        }
    }
}
