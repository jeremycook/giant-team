using GiantTeam.Organizations.Organization.Services;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organizations.Organization.Data
{
    public class OrganizationDbContext : DbContext
    {
        private readonly OrganizationInfo organizationAccessor;
        private readonly UserConnectionService userConnectionService;

        public OrganizationDbContext(OrganizationInfo organizationAccessor, UserConnectionService userConnectionService)
        {
            Spaces = new(this);
            this.organizationAccessor = organizationAccessor;
            this.userConnectionService = userConnectionService;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string databaseName = organizationAccessor.DatabaseName ?? throw new NullReferenceException("The OrganizationAccessor.DatabaseName is null.");
            var connection = userConnectionService.CreateConnection(databaseName);
            options.UseNpgsql(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Spaces.OnModelCreating(modelBuilder);
        }

        public SpacesSchema Spaces { get; }
    }
}
