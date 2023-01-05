using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organizations.Organization.Data
{
    public class OrganizationDbContext : DbContext
    {
        public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : base(options)
        {
            Spaces = new(this);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Spaces.OnModelCreating(modelBuilder);
        }

        public SpacesSchema Spaces { get; }
    }
}
