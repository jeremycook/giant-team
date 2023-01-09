using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GiantTeam.Postgres.InformationSchema.Data;

public class InformationSchemaDbContext : DbContext
{
    public InformationSchemaDbContext(DbContextOptions<InformationSchemaDbContext> options) : base(options)
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
        modelBuilder.HasDefaultSchema("information_schema");
    }

    public DbSet<Tables> Tables { get; } = null!;
}
