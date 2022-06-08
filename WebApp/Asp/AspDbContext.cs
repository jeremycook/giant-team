using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Asp
{
    public class AspDbContext : DbContext, IDataProtectionKeyContext
    {
        public static string DefaultSchema { get; } = "asp";

        public AspDbContext(DbContextOptions<AspDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DefaultSchema);
        }

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    }
}
