using GiantTeam.Postgres;
using Microsoft.Extensions.Configuration;

namespace GiantTeam.Startup.DatabaseConfiguration
{
    public class DatabaseConfigurationsSource : IConfigurationSource
    {
        private readonly ConnectionOptions connectionOptions;

        public DatabaseConfigurationsSource(ConnectionOptions connectionOptions)
        {
            this.connectionOptions = connectionOptions;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DatabaseConfigurationProvider(connectionOptions);
        }
    }
}