namespace WebApp.Services
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
            public const string Username = "username";
            public const string Name = "name";
            public const string Email = "email";
            public const string EmailVerified = "email_verified";
            public const string Role = "role";
            public const string DatabaseUsername = "du";
            public const string DatabaseSlot = "ds";
            public const string DatabasePassword = "dp";
            public const string DatabasePasswordValidUntil = "dv";
        }
    }
}
