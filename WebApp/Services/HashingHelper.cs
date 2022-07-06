using Microsoft.AspNetCore.Identity;

namespace WebApp.Services
{
    public static class HashingHelper
    {
        private static readonly object stubUser = new();
        private static readonly PasswordHasher<object> passwordHasher = new();

        public static string HashPlaintext(string plaintext)
        {
            return passwordHasher.HashPassword(stubUser, plaintext);
        }

        public static bool VerifyHashedPlaintext(string? hashtext, string plaintext)
        {
            return passwordHasher.VerifyHashedPassword(stubUser, hashtext, plaintext) != PasswordVerificationResult.Failed;
        }
    }
}
