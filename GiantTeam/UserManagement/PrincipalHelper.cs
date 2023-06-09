﻿using System.Security.Claims;

namespace GiantTeam.UserManagement
{
    public static class PrincipalHelper
    {
        public static class AuthenticationTypes
        {
            public const string Password = "password";
        }

        public static class ClaimTypes
        {
            public const string Sub = "sub";
            public const string Username = "user";
            public const string Elevated = "elevated";
            public const string Name = "name";
            public const string Email = "email";
            public const string EmailVerified = "email_verified";
            public const string Role = "role";

            public const string DbLogin = "dbl";
            public const string DbPassword = "dbp";
            public const string DbUser = "dbu";
        }

        public static bool Contains(this IEnumerable<Claim> claims, string type)
        {
            return claims.FindValue(type) is not null;
        }

        public static string? FindValue(this IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(o => o.Type == type)?.Value;
        }

        public static string FindRequiredValue(this IEnumerable<Claim> claims, string type)
        {
            return
                claims.FindValue(type) ??
                throw new InvalidOperationException($"Missing required value of \"{type}\" claim.");
        }
    }
}
