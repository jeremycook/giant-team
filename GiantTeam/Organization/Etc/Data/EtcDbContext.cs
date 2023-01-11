using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Organization.Etc.Data;

public class EtcDbContext : DbContext
{
    public EtcDbContext(DbContextOptions<EtcDbContext> options) : base(options)
    {
        Nodes = Set<Node>();
        Files = Set<File>();
        Types = Set<NodeType>();
        TypeConstraints = Set<TypeConstraint>();

        DatabaseDefinitions = Set<DatabaseDefinition>();
        NodePaths = Set<NodePath>();
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

        modelBuilder.Entity<Node>().HasMany(o => o.Children).WithOne().HasForeignKey(o => o.ParentId).OnDelete(DeleteBehavior.NoAction);
    }

    // Tables
    public DbSet<Node> Nodes { get; }
    public DbSet<File> Files { get; }
    public DbSet<NodeType> Types { get; }
    public DbSet<TypeConstraint> TypeConstraints { get; }

    // Views
    public DbSet<DatabaseDefinition> DatabaseDefinitions { get; }
    public DbSet<NodePath> NodePaths { get; }
}
