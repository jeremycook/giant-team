using Microsoft.EntityFrameworkCore;

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
            team.Property(o => o.UsernameLowercase).HasComputedColumnSql("LOWER(username)", stored: true);
            team.Property(o => o.EmailConfirmed).HasDefaultValueSql("false");
            team.Property(o => o.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }

        public DbSet<User> Users => Set<User>();

        public async Task SetUserPasswordAsync(string username, string password, DateTimeOffset validUntil)
        {
            await Database.ExecuteSqlInterpolatedAsync($"CALL gt.set_user_password({username},{password},{validUntil})");
        }
    }
}
