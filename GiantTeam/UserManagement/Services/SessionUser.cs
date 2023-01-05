using GiantTeam.Organizations.Directory.Helpers;
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
                // Common claims
                sub: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.Sub),
                username: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.Username),

                // Database access claims
                dbUser: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.DbUser),
                dbLogin: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.DbLogin),
                dbPassword: identity.Claims.FindRequiredValue(PrincipalHelper.ClaimTypes.DbPassword)
            );

            return sessionUser;
        }

        public SessionUser(
            string sub,
            string username,
            string dbUser,
            string dbLogin,
            string dbPassword)
        {
            Sub = sub ?? throw new ArgumentNullException(nameof(sub));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            DbUser = dbUser ?? throw new ArgumentNullException(nameof(dbUser));
            DbLogin = dbLogin ?? throw new ArgumentNullException(nameof(dbLogin));
            DbPassword = dbPassword ?? throw new ArgumentNullException(nameof(dbPassword));
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
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbUser, DbUser));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbLogin, DbLogin));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbPassword, DbPassword));

            return identity;
        }

        private Guid? _userId;
        private string? _dbRegular;
        private string? _dbElevated;

        public Guid UserId => _userId ??= Guid.Parse(Sub);

        // Common

        public string Sub { get; }
        public string Username { get; }

        // Database

        public string DbUser { get; }
        public string DbLogin { get; }
        public string DbPassword { get; }

        public string DbRegular => _dbRegular ??= DirectoryHelpers.NormalUserRole(DbUser);
        public string DbElevated => _dbElevated ??= DirectoryHelpers.ElevatedUserRole(DbUser);
    }
}