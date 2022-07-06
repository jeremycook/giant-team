using Humanizer;
using Microsoft.EntityFrameworkCore;
using WebApp.Postgres;
using WebApp.Services;

namespace WebApp.Data
{
    public class GiantTeamDbContext : DbContext
    {
        public static string DefaultSchema { get; } = "gt";

        public GiantTeamDbContext(DbContextOptions<GiantTeamDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DefaultSchema);

            var team = modelBuilder.Entity<User>();
            team.Property(o => o.UserId).HasDefaultValueSql();
            team.Property(o => o.Username).HasComputedColumnSql($"LOWER({PgQuote.Identifier(nameof(User.DisplayUsername).Underscore())})", stored: true);
            team.Property(o => o.EmailVerified).HasDefaultValueSql("false");
            team.Property(o => o.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }

        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// Creates the DDL, DML and DQL roles for <paramref name="user"/> that cannot login.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateDatabaseUserRolesAsync(User user)
        {
            await Database.ExecuteSqlRawAsync($@"
CREATE ROLE {PgQuote.Identifier(UserHelper.DDL(user.Username))} WITH
    NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT;

CREATE ROLE {PgQuote.Identifier(UserHelper.DML(user.Username))} WITH
    NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT;

CREATE ROLE {PgQuote.Identifier(UserHelper.DQL(user.Username))} WITH
    NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT;

-- Create corresponding slot 1 logins.
-- TODO: Manage multiple slots.

CREATE USER {PgQuote.Identifier(UserHelper.DDL(user.Username, 1))} WITH
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT
    IN ROLE {PgQuote.Identifier(UserHelper.DDL(user.Username))};

CREATE USER {PgQuote.Identifier(UserHelper.DML(user.Username, 1))} WITH
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT
    IN ROLE {PgQuote.Identifier(UserHelper.DML(user.Username))};

CREATE USER {PgQuote.Identifier(UserHelper.DQL(user.Username, 1))} WITH
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION INHERIT
    IN ROLE {PgQuote.Identifier(UserHelper.DQL(user.Username))};
");
        }

        public async Task SetDatabaseUserPasswordsAsync(string username, int slot, string password, DateTimeOffset validUntil)
        {
            await Database.ExecuteSqlInterpolatedAsync($@"
CALL gt.set_user_password({UserHelper.DDL(username, slot)},{password},{validUntil});
CALL gt.set_user_password({UserHelper.DML(username, slot)},{password},{validUntil});
CALL gt.set_user_password({UserHelper.DQL(username, slot)},{password},{validUntil});
");
        }
    }
}
