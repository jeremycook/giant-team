using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.Services;
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
        /// Creates the DDL, DML and DQL roles for <paramref name="user"/> that cannot login,
        /// and for now also creates the slot 1 logins.
        /// TODO: Develop a system that manages multiple slots.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateDatabaseUserRolesAsync(User user)
        {
            await Database.ExecuteSqlRawAsync($@"
-- Create roles that cannot login
CREATE ROLE {PgQuote.Identifier(DatabaseHelper.DesignUser(user.UsernameNormalized))} WITH
    NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT;

CREATE ROLE {PgQuote.Identifier(DatabaseHelper.ManipulateUser(user.UsernameNormalized))} WITH
    NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT;

CREATE ROLE {PgQuote.Identifier(DatabaseHelper.QueryUser(user.UsernameNormalized))} WITH
    NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT;

-- Create corresponding slot 1 logins.
CREATE USER {PgQuote.Identifier(DatabaseHelper.DesignUser(user.UsernameNormalized, 1))} WITH
    LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT
    IN ROLE {PgQuote.Identifier(DatabaseHelper.DesignUser(user.UsernameNormalized))};

CREATE USER {PgQuote.Identifier(DatabaseHelper.ManipulateUser(user.UsernameNormalized, 1))} WITH
    LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT
    IN ROLE {PgQuote.Identifier(DatabaseHelper.ManipulateUser(user.UsernameNormalized))};

CREATE USER {PgQuote.Identifier(DatabaseHelper.QueryUser(user.UsernameNormalized, 1))} WITH
    LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT
    IN ROLE {PgQuote.Identifier(DatabaseHelper.QueryUser(user.UsernameNormalized))};
");
        }

        public async Task SetDatabaseUserPasswordsAsync(string username, int slot, string password, DateTimeOffset validUntil)
        {
            await Database.ExecuteSqlInterpolatedAsync($@"
CALL wa.set_user_password({DatabaseHelper.DesignUser(username, slot)},{password},{validUntil});
CALL wa.set_user_password({DatabaseHelper.ManipulateUser(username, slot)},{password},{validUntil});
CALL wa.set_user_password({DatabaseHelper.QueryUser(username, slot)},{password},{validUntil});
");
        }
    }
}
