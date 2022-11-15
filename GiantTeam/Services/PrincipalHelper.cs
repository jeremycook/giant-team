namespace GiantTeam.Services
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

            public const string DbLogin = "db_login";
            public const string DbPassword = "db_pass";
            public const string DbRole = "db_role";
        }
    }
}
