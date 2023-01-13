using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Organization.Etc.Data;

public class EtcDbContext : DbContext
{
    public EtcDbContext(DbContextOptions<EtcDbContext> options) : base(options)
    {
        Inodes = Set<Inode>();
        Files = Set<File>();
        InodeTypes = Set<InodeType>();
        InodeTypeConstraints = Set<InodeTypeConstraint>();

        // Keyless
        DatabaseDefinitions = Set<DatabaseDefinition>();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove(typeof(TableNameFromDbSetConvention));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("etc");

        modelBuilder.Entity<Inode>().HasMany(o => o.Children).WithOne().HasForeignKey(o => o.ParentInodeId).OnDelete(DeleteBehavior.NoAction);
    }

    // Tables
    public DbSet<Inode> Inodes { get; }
    public DbSet<File> Files { get; }
    public DbSet<InodeType> InodeTypes { get; }
    public DbSet<InodeTypeConstraint> InodeTypeConstraints { get; }

    // Views
    public DbSet<DatabaseDefinition> DatabaseDefinitions { get; }
}
