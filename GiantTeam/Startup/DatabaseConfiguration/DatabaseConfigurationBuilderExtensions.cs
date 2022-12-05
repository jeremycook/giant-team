using GiantTeam.Postgres;
using Microsoft.Extensions.Configuration;

namespace GiantTeam.Startup.DatabaseConfiguration
{
    public static class DatabaseConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDatabase(
            this IConfigurationBuilder builder,
            ConnectionOptions connectionOptions)
        {
            DatabaseConfigurationsSource source = new(connectionOptions);
            return builder.Add(source);
        }
    }
}
