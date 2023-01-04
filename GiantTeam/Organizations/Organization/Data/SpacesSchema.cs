using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GiantTeam.Organizations.Organization.Data
{
    public class SpacesSchema
    {
        private readonly DbContext dbContext;

        public SpacesSchema(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Database> DatabaseAsync() => await dbContext.Set<Database>().SingleAsync();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entities = new[]
            {
                modelBuilder.Entity<Database>().HasNoKey(),
            };

            foreach (var entity in entities)
            {
                entity.Metadata.SetSchema("spaces");
            }
        }

        public class Database
        {
            public string Name { get; set; } = null!;
            public string Owner { get; set; } = null!;
            public JsonDocument Schemas { get; set; } = null!;
        }
    }
}