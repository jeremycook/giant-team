using GiantTeam.Crypto;
using GiantTeam.Organizations.Directory.Helpers;
using GiantTeam.Organizations.Services;
using GiantTeam.Postgres;

namespace GiantTeam.UserManagement.Services
{
    public class ClusterSecurityService
    {
        private readonly ILogger<ClusterSecurityService> logger;
        private readonly SecurityDataService security;

        public ClusterSecurityService(
            ILogger<ClusterSecurityService> logger,
            SecurityDataService security)
        {
            this.logger = logger;
            this.security = security;
        }

        /// <summary>
        /// Create the regular and elevated database user roles based on <paramref name="dbUser"/>.
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

            var dbElevated = DirectoryHelpers.ElevatedUserRole(dbUser);

            await security.ExecuteAsync(
                $"CREATE ROLE {Sql.Identifier(dbUser)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION IN ROLE {Sql.IdentifierList(DirectoryHelpers.Anyuser)}",
                $"CREATE ROLE {Sql.Identifier(dbElevated)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION IN ROLE {Sql.IdentifierList(dbUser)}");

            logger.LogInformation("Created database user roles {DbUser}, {DbElevated}.", dbUser, dbElevated);
        }

        /// <summary>
        /// Create a new database login for <paramref name="dbUser"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateLoginAsync(string dbUser, string dbLogin, string loginPassword, DateTimeOffset validUntil)
        {
            if (!dbUser.StartsWith("u:"))
            {
                throw new ArgumentException($"The {nameof(dbUser)} argument must start with \"u:\".", nameof(dbUser));
            }

            var encryptedPassword = SCRAMSHA256.EncryptPassword(loginPassword);

            await security.ExecuteAsync(
// Create a login for the user's database user
$"""
CREATE ROLE {Sql.Identifier(dbLogin)} WITH LOGIN NOINHERIT NOCREATEDB NOSUPERUSER NOCREATEROLE NOREPLICATION
    IN ROLE {Sql.Identifier(dbUser)}
    ENCRYPTED PASSWORD {Sql.Literal(encryptedPassword)}
    VALID UNTIL {Sql.Literal(validUntil)}
""",

// Have the login default to the user role upon login
$"""
ALTER ROLE {Sql.Identifier(dbLogin)} SET ROLE {Sql.Identifier(dbUser)}
""");

            logger.LogInformation("Created database login {LoginRole} for {DbUser} that is valid until {ValidUntil}.",
                dbUser,
                dbLogin,
                validUntil);
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
            await security.ExecuteAsync($"ALTER ROLE {Sql.Identifier(user.DbLogin)} VALID UNTIL {Sql.Literal(validUntil)}");
            logger.LogInformation("Changed the expiration of database login {LoginRole} to be valid until {ValidUntil}.",
                user.DbLogin,
                validUntil);

            if (user.DbElevatedLogin is not null)
            {
                await security.ExecuteAsync($"ALTER ROLE {Sql.Identifier(user.DbElevatedLogin)} VALID UNTIL {Sql.Literal(validUntil)}");
                logger.LogInformation("Changed the expiration of database elevated login {LoginRole} to be valid until {ValidUntil}.",
                    user.DbElevatedLogin,
                    validUntil);
            }
        }
    }
}
