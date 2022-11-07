using Humanizer;
using Microsoft.EntityFrameworkCore;
using WebApp.Postgres;
using WebApp.Services;

namespace WebApp.Data
{
    public class GiantTeamDbContext : DbContext
    {
        /// <summary>
        /// "gt"
        /// </summary>
        public static string DefaultSchema { get; } = "gt";

        public GiantTeamDbContext(DbContextOptions<GiantTeamDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DefaultSchema);

            var user = modelBuilder.Entity<User>();
            user.Property(o => o.UserId).HasDefaultValueSql();
            user.Property(o => o.UsernameNormalized).HasComputedColumnSql($"LOWER({PgQuote.Identifier(nameof(User.Username).Underscore())})", stored: true);
            user.Property(o => o.EmailVerified).HasDefaultValueSql("false");
            user.Property(o => o.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }

        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<User> Users => Set<User>();

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
CALL gt.set_user_password({DatabaseHelper.DesignUser(username, slot)},{password},{validUntil});
CALL gt.set_user_password({DatabaseHelper.ManipulateUser(username, slot)},{password},{validUntil});
CALL gt.set_user_password({DatabaseHelper.QueryUser(username, slot)},{password},{validUntil});
");
        }
    }
}
