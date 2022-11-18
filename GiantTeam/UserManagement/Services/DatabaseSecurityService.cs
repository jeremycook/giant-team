using Dapper;
using GiantTeam.Crypto;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.UserManagement.Services
{
    public class DatabaseSecurityService
    {
        private readonly SecurityConnectionService security;

        public DatabaseSecurityService(SecurityConnectionService security)
        {
            this.security = security;
        }

        /// <summary>
        /// Create a team role that inherits, cannot login and has an admin.
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="adminName"></param>
        /// <returns></returns>
        public async Task CreateTeamAsync(string teamName, string adminName)
        {
            using var connection = await security.OpenConnectionAsync();
            await connection.ExecuteAsync($"""
-- Create a team role that inherits, cannot login, and has an admin
CREATE ROLE {PgQuote.Identifier(teamName)} 
    WITH NOLOGIN INHERIT
    ADMIN {PgQuote.Identifier(adminName)};
""");
        }

        /// <summary>
        /// Create a user role that inherits but cannot login.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task CreateUserAsync(string userName)
        {
            using var connection = await security.OpenConnectionAsync();
            await connection.ExecuteAsync($"""
-- Create a user role that cannot login
CREATE ROLE {PgQuote.Identifier(userName)}
    WITH NOLOGIN INHERIT;
""");
        }

        /// <summary>
        /// Create a new database login for <paramref name="user"/>.
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
    VALID UNTIL {PgQuote.Literal(validUntil.ToString("u"))};

-- Have login set the user role upon login
ALTER ROLE {PgQuote.Identifier(loginRole)}
    SET ROLE {PgQuote.Identifier(userRole)};
""");
        }

        public async Task SetLoginExpirationAsync(SessionUser user, DateTimeOffset validUntil)
        {
            using var connection = await security.OpenConnectionAsync();
            await connection.ExecuteAsync($"""
-- Set login expiration
ALTER ROLE {PgQuote.Identifier(user.DbLogin)}
    VALID UNTIL {PgQuote.Literal(validUntil.ToString("u"))};
""");
        }
    }
}
