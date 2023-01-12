using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Organization.Etc.Data;

public class EtcDbContext : DbContext
{
    public EtcDbContext(DbContextOptions<EtcDbContext> options) : base(options)
    {
        Datums = Set<Datum>();
        Files = Set<File>();
        Types = Set<DatumType>();
        TypeConstraints = Set<TypeConstraint>();

        DatabaseDefinitions = Set<DatabaseDefinition>();
        DatumPaths = Set<DatumPath>();
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

        modelBuilder.Entity<Datum>().HasMany(o => o.Children).WithOne().HasForeignKey(o => o.ParentId).OnDelete(DeleteBehavior.NoAction);
    }

    // Tables
    public DbSet<Datum> Datums { get; }
    public DbSet<File> Files { get; }
    public DbSet<DatumType> Types { get; }
    public DbSet<TypeConstraint> TypeConstraints { get; }

    // Views
    public DbSet<DatabaseDefinition> DatabaseDefinitions { get; }
    public DbSet<DatumPath> DatumPaths { get; }
}
