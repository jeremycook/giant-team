using GiantTeam.Organizations.Organization.Data.Spaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiantTeam.Organizations.Organization.Data
{
    public class SpacesSchema
    {
        private readonly DbContext dbContext;

        public SpacesSchema(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DbSet<Space> Spaces => dbContext.Set<Space>();

        public async Task<Database> DatabaseAsync() => await dbContext.Set<Database>().SingleAsync();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entities = new EntityTypeBuilder[]
            {
                modelBuilder.Entity<Space>(),
                modelBuilder.Entity<Database>().HasNoKey(),
            };

            foreach (var entity in entities)
            {
                entity.Metadata.SetSchema(OrganizationHelper.SpacesSchema);
            }
        }
    }
}