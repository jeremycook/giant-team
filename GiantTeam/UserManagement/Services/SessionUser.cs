using System.Security.Claims;

namespace GiantTeam.UserManagement.Services
{
    public class SessionUser
    {
        public static SessionUser Create(ClaimsIdentity identity)
        {
            if (identity is null)
                throw new ArgumentNullException(nameof(identity));

            SessionUser sessionUser = new(
                sub:
                    identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.Sub)?.Value ??
                    throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.Sub}\" claim."),

                username:
                    identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.Username)?.Value ??
                    throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.Username}\" claim."),

                name:
                    identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.Name)?.Value ??
                    throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.Name}\" claim."),

                // Database stuff

                dbLogin:
                    identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.DbLogin)?.Value ??
                    throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.DbLogin}\" claim."),

                dbPassword:
                    identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.DbPassword)?.Value ??
                    throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.DbPassword}\" claim."),

                dbRole:
                    identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.DbRole)?.Value ??
                    throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.DbRole}\" claim.")
            );

            return sessionUser;
        }

        public SessionUser(
            string sub,
            string username,
            string name,
            string dbLogin,
            string dbPassword,
            string dbRole)
        {
            Sub = sub;
            Username = username;
            Name = name;

            DbLogin = dbLogin;
            DbPassword = dbPassword;
            DbRole = dbRole;
        }

        public ClaimsIdentity CreateIdentity(string authenticationType)
        {
            ClaimsIdentity identity = new(
                authenticationType: authenticationType,
                nameType: PrincipalHelper.ClaimTypes.Username,
                roleType: PrincipalHelper.ClaimTypes.Role
            );

            // Common claims
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Sub, Sub));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Name, Name));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Username, Username));

            // Database claims
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbLogin, DbLogin));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbPassword, DbPassword));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbRole, DbRole));

            return identity;
        }

        private Guid? _userId;

        public Guid UserId => _userId ??= Guid.Parse(Sub);

        // Common

        public string Sub { get; }
        public string Username { get; }
        public string Name { get; }

        // Database

        public string DbLogin { get; }
        public string DbPassword { get; }
        public string DbRole { get; }
    }
}