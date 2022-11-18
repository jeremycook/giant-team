using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.WorkspaceAdministration.Data
{
    public class WorkspaceAdministrationDbContext : DbContext
    {
        public const string DefaultSchema = "wa";

        public WorkspaceAdministrationDbContext(DbContextOptions<WorkspaceAdministrationDbContext> options)
            : base(options) { }

        protected WorkspaceAdministrationDbContext()
            : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DefaultSchema);
        }

        /// <summary>
        /// Create a group role.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="adminRole"></param>
        /// <returns></returns>
        public async Task CreateDatabaseGroupAsync(string group, string adminRole)
        {
            await Database.ExecuteSqlRawAsync($"""
-- Create a group role
CREATE ROLE {PgQuote.Identifier(group)} WITH NOLOGIN INHERIT ADMIN {PgQuote.Identifier(adminRole)};
""");
        }

        /// <summary>
        /// Create a user role that cannot login.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateDatabaseUserAsync(string user)
        {
            await Database.ExecuteSqlRawAsync($"""
-- Create a user role that cannot login
CREATE ROLE {PgQuote.Identifier(user)} WITH NOLOGIN INHERIT;
""");
        }

        /// <summary>
        /// Create a new login for a user and returns its name.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string> CreateDatabaseLoginAsync(string user)
        {
            string login = $"{user}:l:{DateTime.UtcNow:yymmddHHmmss}";
            await Database.ExecuteSqlRawAsync($"""
-- Create a login for a user
CREATE ROLE {PgQuote.Identifier(login)} WITH LOGIN NOINHERIT IN ROLE {PgQuote.Identifier(user)};
ALTER ROLE {PgQuote.Identifier(login)} SET ROLE {PgQuote.Identifier(user)};
""");
            return login;
        }

        public async Task SetDatabasePasswordsAsync(string login, string password, DateTimeOffset validUntil)
        {
            await Database.ExecuteSqlInterpolatedAsync($"""
CALL wa.set_user_password({login},{password},{validUntil});
""");
        }
    }
}
