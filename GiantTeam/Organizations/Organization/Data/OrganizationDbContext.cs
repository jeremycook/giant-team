using GiantTeam.Organizations.Organization.Data.InformationSchema;
using GiantTeam.Organizations.Organization.Data.Spaces;
using GiantTeam.Text;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organizations.Organization.Data
{
    public class OrganizationDbContext : DbContext
    {
        public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : base(options)
        {
            InformationSchema = new(this);
            Spaces = new(this);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var schemaProperty in typeof(OrganizationDbContext)
                .GetProperties()
                .Where(p => p.PropertyType.Name.EndsWith("Schema")))
            {
                string schemaName = TextTransformers.Snakify(schemaProperty.Name);

                foreach (var dbSetCandidate in schemaProperty.PropertyType
                    .GetProperties())
                {
                    if (dbSetCandidate.PropertyType.IsGenericType &&
                        dbSetCandidate.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    {
                        var entityType = dbSetCandidate.PropertyType.GetGenericArguments()[0];
                        var entity = modelBuilder.Entity(entityType);
                        if (entity.Metadata.GetSchema() is null)
                        {
                            entity.Metadata.SetSchema(schemaName);
                        }
                    }
                }
            }
        }

        public InformationSchemaSchema InformationSchema { get; }
        public SpacesSchema Spaces { get; }
    }
}
