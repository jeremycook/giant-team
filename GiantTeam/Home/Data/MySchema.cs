using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Home.Data
{
    public class MySchema
    {
        private DbContext dbContext;

        public MySchema(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DbSet<Database> Databases => dbContext.Set<Database>();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entities = new[]
            {
                modelBuilder.Entity<Database>(),
            };

            foreach (var entity in entities)
            {
                entity.Metadata.SetSchema("my");
            }
        }

        public class Database
        {
            public string Name { get; set; } = null!;
        }
    }
}