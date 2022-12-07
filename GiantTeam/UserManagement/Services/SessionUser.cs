using System.Security.Claims;

namespace GiantTeam.UserManagement.Services
{
    public class SessionUser
    {
        public static SessionUser Create(ClaimsIdentity identity)
        {
            if (identity is null)
                throw new ArgumentNullException(nameof(identity));

            string dbLogin = identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.DbLogin);
            string dbRole = dbLogin[..dbLogin.IndexOf(':')];

            SessionUser sessionUser = new(
                // Common claims
                sub: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.Sub),
                username: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.Username),

                // Database access claims
                dbRole: dbRole,
                dbLogin: dbLogin,
                dbPassword: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.DbPassword)
            );

            return sessionUser;
        }

        public SessionUser(
            string sub,
            string username,
            string dbRole,
            string dbLogin,
            string dbPassword)
        {
            Sub = sub ?? throw new ArgumentNullException(nameof(sub));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            DbRole = dbRole ?? throw new ArgumentNullException(nameof(dbRole));
            DbLogin = dbLogin ?? throw new ArgumentNullException(nameof(dbLogin));
            DbPassword = dbPassword ?? throw new ArgumentNullException(nameof(dbPassword));

            if (!DbLogin.StartsWith(DbRole + ':'))
            {
                throw new ArgumentException($"The {nameof(DbLogin)} must start with {DbRole + ':'}.");
            }
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
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Username, Username));

            // Database claims
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbLogin, DbLogin));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbPassword, DbPassword));

            return identity;
        }

        private Guid? _userId;

        public Guid UserId => _userId ??= Guid.Parse(Sub);

        // Common

        public string Sub { get; }
        public string Username { get; }

        // Database

        public string DbRole { get; }
        public string DbLogin { get; }
        public string DbPassword { get; }
    }
}