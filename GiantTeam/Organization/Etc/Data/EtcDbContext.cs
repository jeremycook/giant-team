using GiantTeam.Organization.Etc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Organization.Etc.Data;

public class EtcDbContext : DbContext
{
    public static readonly string Schema = "etc";

    public EtcDbContext(DbContextOptions<EtcDbContext> options) : base(options)
    {
        Inodes = Set<InodeRecord>();
        Files = Set<FileRecord>();
        InodeTypes = Set<InodeTypeRecord>();
        InodeTypeConstraints = Set<InodeTypeConstraintRecord>();

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
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<InodeRecord>().HasMany<InodeRecord>().WithOne().HasForeignKey(o => o.ParentInodeId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<FileRecord>().HasOne<InodeRecord>().WithOne().HasPrincipalKey<InodeRecord>(o => o.InodeId).OnDelete(DeleteBehavior.Cascade);
    }

    // Tables

    public DbSet<InodeRecord> Inodes { get; }
    public DbSet<FileRecord> Files { get; }
    public DbSet<InodeAccessRecord> InodeAccesses { get; }

    public DbSet<InodeTypeRecord> InodeTypes { get; }
    public DbSet<InodeTypeConstraintRecord> InodeTypeConstraints { get; }

    public DbSet<RoleRecord> Roles { get; }

    // Views

    public DbSet<DatabaseDefinition> DatabaseDefinitions { get; }
}
