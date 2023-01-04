using GiantTeam.Postgres;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Services
{
    public class SecurityDataService : PgDataService
    {
        private readonly IOptions<GiantTeamOptions> options;
        private string? _connectionString;

        public SecurityDataService(IOptions<GiantTeamOptions> options)
        {
            this.options = options;
        }

        public override string ConnectionString
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
