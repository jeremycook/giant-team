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

            var user = modelBuilder.Entity<User>();
            user.Property(o => o.UserId).HasDefaultValueSql();
            user.Property(o => o.UsernameNormalized).HasComputedColumnSql($"LOWER({PgQuote.Identifier(nameof(User.Username))})", stored: true);
            user.Property(o => o.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }

        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<User> Users => Set<User>();
    }
}
