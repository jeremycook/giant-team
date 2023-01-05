using GiantTeam.Postgres;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Services
{
    public class SecurityDataService : PgDataServiceBase
    {
        private readonly IOptions<GiantTeamOptions> options;
        private string? _connectionString;

        public SecurityDataService(
            ILogger<SecurityDataService> logger,
            IOptions<GiantTeamOptions> options)
        {
            Logger = logger;
            this.options = options;
        }

        protected override ILogger Logger { get; }

        protected override string ConnectionString
        {
            get
            {
                if (_connectionString is null)
                {
                    var connectionOptions = options.Value.SecurityConnection;
                    NpgsqlConnectionStringBuilder connectionStringBuilder = connectionOptions.ToConnectionStringBuilder();
                    _connectionString = connectionStringBuilder.ToString();
                }

                return _connectionString;
            }
        }
    }
}
