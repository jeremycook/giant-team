using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Asp
{
    public class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        /// <summary>
        /// "dp"
        /// </summary>
        public static string DefaultSchema { get; } = "dp";

        public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DefaultSchema);
        }

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    }
}
