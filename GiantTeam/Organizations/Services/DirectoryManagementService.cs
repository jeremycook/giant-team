using GiantTeam.Postgres;
using Microsoft.Extensions.Options;

namespace GiantTeam.Organizations.Services
{
    public class DirectoryManagementService : PgDataService
    {
        private readonly IOptions<GiantTeamOptions> options;
        private string? _connectionString;

        protected override string ConnectionString
        {
            get
            {
                if (_connectionString is null)
                {
                    var connectionOptions = options.Value.DirectoryManagerConnection;
                    var connectionStringBuilder = connectionOptions.ToConnectionStringBuilder();
                    _connectionString = connectionStringBuilder.ToString();
                }

                return _connectionString;
            }
        }

        protected override ILogger Logger { get; }

        public DirectoryManagementService(
            ILogger<DirectoryManagementService> logger,
            IOptions<GiantTeamOptions> options)
        {
            Logger = logger;
            this.options = options;
        }
    }
}
