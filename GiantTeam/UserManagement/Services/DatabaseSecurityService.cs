using Dapper;
using GiantTeam.Crypto;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.UserManagement.Services
{
    public class DatabaseSecurityService
    {
        private readonly ILogger<DatabaseSecurityService> logger;
        private readonly SecurityConnectionService security;

        public DatabaseSecurityService(
            ILogger<DatabaseSecurityService> logger,
            SecurityConnectionService security)
        {
            this.logger = logger;
            this.security = security;
        }

        /// <summary>
        /// Create a team role with an admin <see cref="SessionUser.DbRole"/> of <paramref name="adminUser"/> that inherits, and cannot login.
        /// </summary>
        /// <param name="teamRole"></param>
        /// <param name="adminRole"></param>
        /// <returns></returns>
        public async Task CreateTeamAsync(string teamRole, SessionUser adminUser)
        {
            string adminRole = adminUser.DbRole;

            using var connection = await security.OpenConnectionAsync();
            await connection.ExecuteAsync($"""
-- Create a team role that inherits, cannot login, and has an admin
CREATE ROLE {PgQuote.Identifier(teamRole)} 
    WITH NOLOGIN INHERIT
    ADMIN {PgQuote.Identifier(adminRole)};
""");
            logger.LogInformation("Created team {TeamRole} with admin {AdminRole}.", teamRole, adminRole);
        }

        /// <summary>
        /// Create a user role that inherits, can create databases, and cannot login.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task CreateUserAsync(string userName)
        {
            using var connection = await security.OpenConnectionAsync();
            await connection.ExecuteAsync($"""
-- Create a user role that cannot login
CREATE ROLE {PgQuote.Identifier(userName)}
    WITH NOLOGIN INHERIT CREATEDB;
""");
            logger.LogInformation("Created user {UserRole}.", userName);
        }

        /// <summary>
        /// Create a new database login for <paramref name="user"/> that can login, does not inherit,
        /// and is valid until <paramref name="validUntil"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateLoginAsync(SessionUser user, DateTimeOffset validUntil)
        {
            string loginRole = user.DbLogin;
            string userRole = user.DbRole;
            var encryptedPassword = SCRAMSHA256.EncryptPassword(user.DbPassword);

            using var connection = await security.OpenConnectionAsync();
            await connection.ExecuteAsync($"""
-- Create a login for a user
CREATE ROLE {PgQuote.Identifier(loginRole)}
    WITH LOGIN NOINHERIT
    IN ROLE {PgQuote.Identifier(userRole)}
    ENCRYPTED PASSWORD {PgQuote.Literal(encryptedPassword)}
    VALID UNTIL {PgQuote.Literal(validUntil)};

-- Have login set the user role upon login
ALTER ROLE {PgQuote.Identifier(loginRole)}
    SET ROLE {PgQuote.Identifier(userRole)};
""");

            logger.LogInformation("Created login {LoginRole} for {UserRole} that is valid until {ValidUntil}.", loginRole, userRole, validUntil);
        }

        /// <summary>
        /// Extend or expire the password of <paramref name="user"/>'s <see cref="SessionUser.DbLogin"/>
        /// to <paramref name="validUntil"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="validUntil"></param>
        /// <returns></returns>
        public async Task SetLoginExpirationAsync(SessionUser user, DateTimeOffset validUntil)
        {
            string loginRole = user.DbLogin;

            using var connection = await security.OpenConnectionAsync();
            await connection.ExecuteAsync($"""
-- Set login expiration
ALTER ROLE {PgQuote.Identifier(loginRole)}
    VALID UNTIL {PgQuote.Literal(validUntil)};
""");
            logger.LogInformation("Changed the password of login {LoginRole} to be valid until {ValidUntil}.", loginRole, validUntil);
        }
    }
}
