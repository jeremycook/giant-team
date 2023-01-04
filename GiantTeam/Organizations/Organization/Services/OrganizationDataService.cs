using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Organization.Services
{
    public class OrganizationDataService : PgDataService
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;
        private readonly OrganizationInfo organization;
        private readonly SpaceInfo space;
        private string? _connectionString;

        public override string ConnectionString
        {
            get
            {
                if (_connectionString is null)
                {
                    var connectionOptions = options.Value.DirectoryConnection;
                    var user = sessionService.User;

                    NpgsqlConnectionStringBuilder connectionStringBuilder = connectionOptions.ToConnectionStringBuilder();
                    connectionStringBuilder.Database = organization.DatabaseName ?? throw new NullReferenceException("The OrganizationAccessor.DatabaseName property is null.");
                    connectionStringBuilder.SearchPath = space.SchemaName ?? "spaces";
                    connectionStringBuilder.Username = user.DbLogin;
                    connectionStringBuilder.Password = user.DbPassword;

                    _connectionString = connectionStringBuilder.ToString();
                }

                return _connectionString;
            }
        }

        public OrganizationDataService(IOptions<GiantTeamOptions> options, SessionService sessionService, OrganizationInfo organization, SpaceInfo space)
        {
            this.options = options;
            this.sessionService = sessionService;
            this.organization = organization;
            this.space = space;
        }
    }
}
