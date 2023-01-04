using GiantTeam.Crypto;
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
        /// Create a user role that inherits but has no other privileges, and an admin role that has no privileges.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task CreateUserRolesAsync(string userRole, string adminRole)
        {
            await security.ExecuteAsync(
                $"CREATE ROLE {Sql.Identifier(userRole)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION",
                $"CREATE ROLE {Sql.Identifier(adminRole)} WITH NOINHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION");

            logger.LogInformation("Created database user role {UserRole} and admin role {AdminRole}.", userRole, adminRole);
        }

        /// <summary>
        /// Create a new database login for <paramref name="user"/> that can login, inherits,
        /// and is valid until <paramref name="validUntil"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateLoginAsync(SessionUser user, DateTimeOffset validUntil)
        {
            string loginRole = user.DbLogin;
            string userRole = user.DbRole;
            string adminRole = user.DbAdmin;
            var encryptedPassword = SCRAMSHA256.EncryptPassword(user.DbPassword);

            await security.ExecuteAsync(
// Create a login for the user's database user and admin roles
$"""
CREATE ROLE {Sql.Identifier(loginRole)} WITH LOGIN NOINHERIT NOCREATEDB NOSUPERUSER NOCREATEROLE NOREPLICATION
    IN ROLE {Sql.Identifier(userRole)}, {Sql.Identifier(adminRole)}
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
