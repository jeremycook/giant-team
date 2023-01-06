using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Data.InformationSchema
{
    public class InformationSchemaSchema
    {
        private readonly DbContext dbContext;

        public InformationSchemaSchema(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DbSet<Tables> Tables => dbContext.Set<Tables>();
    }
}
