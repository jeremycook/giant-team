using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Organization.Etc.Data;

public class EtcDbContext : DbContext
{
    public EtcDbContext(DbContextOptions<EtcDbContext> options) : base(options)
    {
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
    }

    // Tables
    public DbSet<Node> Nodes { get; } = null!;
    public DbSet<File> Files { get; } = null!;
    public DbSet<NodeType> Types { get; } = null!;
    public DbSet<TypeConstraint> TypeConstraints { get; } = null!;

    // Views
    public DbSet<DatabaseDefinition> DatabaseDefinitions { get; } = null!;
}
