using GiantTeam.Cluster.Directory.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Cluster.Directory.Data;

public abstract class DirectoryDbContext<T> : DbContext where T : DirectoryDbContext<T>
{
    public DirectoryDbContext(DbContextOptions<T> options)
        : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove(typeof(TableNameFromDbSetConvention));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DirectoryHelpers.Schema);

        var userPasswords = modelBuilder.Entity<UserPassword>();
        userPasswords.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();
    internal DbSet<UserPassword> UserPasswords => Set<UserPassword>();
}
