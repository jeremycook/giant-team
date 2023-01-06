using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organizations.Organization.Data.Spaces
{
    public class SpacesSchema
    {
        private readonly DbContext dbContext;

        public SpacesSchema(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DbSet<DatabaseDefinition> DatabaseDefinitions => dbContext.Set<DatabaseDefinition>();
        public DbSet<Space> Spaces => dbContext.Set<Space>();
    }
}
