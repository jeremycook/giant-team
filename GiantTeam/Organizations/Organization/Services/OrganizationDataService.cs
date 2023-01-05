using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Organization.Services
{
    public class OrganizationDataService : PgDataServiceBase
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;
        private readonly OrganizationInfo organization;
        private readonly SpaceInfo space;
        private string? _connectionString;

        protected override ILogger Logger { get; }

        protected override string ConnectionString
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

        public OrganizationDataService(
            ILogger<OrganizationDataService> logger,
            IOptions<GiantTeamOptions> options,
            SessionService sessionService,
            OrganizationInfo organization,
            SpaceInfo space)
        {
            Logger = logger;
            this.options = options;
            this.sessionService = sessionService;
            this.organization = organization;
            this.space = space;
        }
    }
}
