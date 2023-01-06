using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace GiantTeam.Crypto
{
    public static class PasswordHelper
    {
        private const int iterationCount = 300_000;
        private static readonly object stubUser = new();
        private static readonly PasswordHasher<object> passwordHasher = new(Options.Create<PasswordHasherOptions>(new() { IterationCount = iterationCount }));

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
        /// Returns <see cref="VerifyPlaintextResult.Success"/> or <see cref="VerifyPlaintextResult.SuccessRehashNeeded"/> if the <paramref name="plaintext"/> is valid.
        /// </summary>
        /// <param name="hashtext"></param>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static VerifyPlaintextResult VerifyHashedPlaintext(string hashtext, string plaintext)
        {
            return (VerifyPlaintextResult)passwordHasher.VerifyHashedPassword(stubUser, hashtext, plaintext);
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
                throw new ArgumentOutOfRangeException(nameof(bitsOfEntropy), bitsOfEntropy, $"The {nameof(bitsOfEntropy)} must be at least 128.");

            int numberOfBytes = (int)Math.Ceiling(bitsOfEntropy / 8f);
            byte[] bytes = RandomNumberGenerator.GetBytes(numberOfBytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }
    }
}
