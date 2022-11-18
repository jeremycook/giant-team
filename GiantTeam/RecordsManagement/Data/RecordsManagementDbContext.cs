using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.RecordsManagement.Data
{
    public class RecordsManagementDbContext : DbContext
    {
        public const string DefaultSchema = "rm";

        public RecordsManagementDbContext(DbContextOptions<RecordsManagementDbContext> options)
            : base(options) { }

        protected RecordsManagementDbContext()
            : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DefaultSchema);

            var dbRole = modelBuilder.Entity<DbRole>();
            dbRole.Property(o => o.Created).HasDefaultValueSql();

            var user = modelBuilder.Entity<User>();
            user.Property(o => o.UserId).HasDefaultValueSql();
            user.Property(o => o.InvariantUsername).HasComputedColumnSql($"LOWER({PgQuote.Identifier(nameof(User.Username))})", stored: true);
            user.Property(o => o.Created).HasDefaultValueSql();
        }

        public DbSet<DbRole> DbRoles => Set<DbRole>();
        public DbSet<User> Users => Set<User>();
        internal DbSet<UserPassword> UserPasswords => Set<UserPassword>();
    }
}
