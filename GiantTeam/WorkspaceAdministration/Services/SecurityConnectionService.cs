using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.WorkspaceAdministration.Services
{
    /// <summary>
    /// Create connections based on <see cref="GiantTeamOptions.SecurityConnection"/>.
    /// </summary>
    public class SecurityConnectionService
    {
        private readonly IOptions<GiantTeamOptions> options;

        public SecurityConnectionService(IOptions<GiantTeamOptions> options)
        {
            this.options = options;
        }

        /// <summary>
        /// Returns a new and open connection based on <see cref="GiantTeamOptions.SecurityConnection"/>.
        /// </summary>
        /// <returns></returns>
        public NpgsqlConnection OpenConnection()
        {
            var adminConnectionOptions = options.Value.SecurityConnection;
            var connection = adminConnectionOptions.CreateOpenConnection();
            return connection;
        }

        /// <summary>
        /// Returns a new and open connection based on <see cref="GiantTeamOptions.SecurityConnection"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var adminConnectionOptions = options.Value.SecurityConnection;
            var connection = await adminConnectionOptions.CreateOpenConnectionAsync();
            return connection;
        }
    }
}
