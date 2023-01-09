using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Organization.Spaces.Data;

public class SpacesDbContext : DbContext
{
    public SpacesDbContext(DbContextOptions<SpacesDbContext> options) : base(options)
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
        modelBuilder.HasDefaultSchema("spaces");
    }

    public DbSet<DatabaseDefinition> DatabaseDefinitions { get; } = null!;
    public DbSet<Space> Spaces { get; } = null!;
}
