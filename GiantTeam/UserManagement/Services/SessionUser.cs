using GiantTeam.Cluster.Directory.Helpers;
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
                elevated: identity.Claims.Contains(PrincipalHelper.ClaimTypes.Elevated),

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
            bool elevated,
            string dbUser,
            string dbLogin,
            string dbPassword)
        {
            Sub = sub ?? throw new ArgumentNullException(nameof(sub));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Elevated = elevated;
            DbUser = dbUser ?? throw new ArgumentNullException(nameof(dbUser));
            DbLogin = dbLogin ?? throw new ArgumentNullException(nameof(dbLogin));
            _dbPassword = dbPassword ?? throw new ArgumentNullException(nameof(dbPassword));
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

            if (Elevated)
            {
                identity.AddClaim(new(PrincipalHelper.ClaimTypes.Elevated, "t"));
            }

            // Database claims
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbUser, DbUser));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbLogin, DbLogin));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DbPassword, DbPassword()));

            return identity;
        }

        private Guid? _userId;
        private string? _dbElevatedUser;
        private string? _dbElevatedLogin;
        private readonly string _dbPassword;

        // Common

        public string Sub { get; }
        public string Username { get; }
        public bool Elevated { get; }

        public Guid UserId => _userId ??= Guid.Parse(Sub);

        // Database

        public string DbUser { get; }
        public string DbLogin { get; }

        public string? DbElevatedUser => _dbElevatedUser ??= (Elevated ? DirectoryHelpers.ElevatedUserRole(DbUser) : null);
        public string? DbElevatedLogin => _dbElevatedLogin ??= (Elevated ? DirectoryHelpers.ElevatedLogin(DbLogin) : null);

        /// <summary>
        /// Protect against accidental serialization.
        /// </summary>
        /// <returns></returns>
        public string DbPassword() => _dbPassword;
    }
}