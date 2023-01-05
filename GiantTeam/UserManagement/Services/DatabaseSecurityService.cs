using GiantTeam.Crypto;
using GiantTeam.Organizations.Directory.Data;
using GiantTeam.Organizations.Directory.Helpers;
using GiantTeam.Organizations.Services;
using GiantTeam.Postgres;

namespace GiantTeam.UserManagement.Services
{
    public class DatabaseSecurityService
    {
        private readonly ILogger<DatabaseSecurityService> logger;
        private readonly SecurityDataService security;

        public DatabaseSecurityService(
            ILogger<DatabaseSecurityService> logger,
            SecurityDataService security)
        {
            this.logger = logger;
            this.security = security;
        }

        /// <summary>
        /// Create database user roles based on <paramref name="dbUser"/>.
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task CreateUserRolesAsync(string dbUser)
        {
            if (!dbUser.StartsWith("u:"))
            {
                throw new ArgumentException($"The {nameof(dbUser)} argument must start with \"u:\".", nameof(dbUser));
            }

            var dbNormal = DirectoryHelpers.NormalUserRole(dbUser);
            var dbElevated = DirectoryHelpers.ElevatedUserRole(dbUser);

            await security.ExecuteAsync(
                $"CREATE ROLE {Sql.Identifier(dbNormal)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION",
                $"CREATE ROLE {Sql.Identifier(dbElevated)} WITH NOINHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION",
                $"CREATE ROLE {Sql.Identifier(dbUser)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION IN ROLE {Sql.IdentifierList(dbNormal, dbElevated, DirectoryHelpers.Anyuser)}");

            logger.LogInformation("Created database user roles {DbUser}, {DbNormal}, {DbElevated}.", dbUser, dbNormal, dbElevated);
        }

        /// <summary>
        /// Create a new database login for <paramref name="user"/> that can login, inherits,
        /// and is valid until <paramref name="validUntil"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateLoginAsync(User user, string loginRole, string loginPassword, DateTimeOffset validUntil)
        {
            string userRole = user.DbUser;
            var encryptedPassword = SCRAMSHA256.EncryptPassword(loginPassword);

            await security.ExecuteAsync(
// Create a login for the user's database user
$"""
CREATE ROLE {Sql.Identifier(loginRole)} WITH LOGIN NOINHERIT NOCREATEDB NOSUPERUSER NOCREATEROLE NOREPLICATION
    IN ROLE {Sql.Identifier(userRole)}
    ENCRYPTED PASSWORD {Sql.Literal(encryptedPassword)}
    VALID UNTIL {Sql.Literal(validUntil)}
""",

// Have the login default the user role upon login
$"""
ALTER ROLE {Sql.Identifier(loginRole)} SET ROLE {Sql.Identifier(userRole)}
""");

            logger.LogInformation("Created database login {LoginRole} for {UserRole} that is valid until {ValidUntil}.", loginRole, userRole, validUntil);
        }

        /// <summary>
        /// Extend or expire the password of the <paramref name="user"/>'s <see cref="SessionUser.DbLogin"/>
        /// to <paramref name="validUntil"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="validUntil"></param>
        /// <returns></returns>
        public async Task SetLoginExpirationAsync(SessionUser user, DateTimeOffset validUntil)
        {
            string loginRole = user.DbLogin;

            await security.ExecuteAsync($"ALTER ROLE {Sql.Identifier(loginRole)} VALID UNTIL {Sql.Literal(validUntil)}");
            logger.LogInformation("Changed the expiration of database login {LoginRole} to be valid until {ValidUntil}.", loginRole, validUntil);
        }
    }
}
