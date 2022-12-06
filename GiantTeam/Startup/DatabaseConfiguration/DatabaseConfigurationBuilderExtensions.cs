using GiantTeam.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GiantTeam.Startup.DatabaseConfiguration
{
    public static class DatabaseConfigurationBuilderExtensions
    {
        /// <summary>
        /// Add configuration from database based on 
        /// ConfigurationConnectionFile or ConfigurationConnection
        /// configuration sections.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="ApplicationException"></exception>
        public static IConfigurationBuilder AddDatabase(
            this IConfigurationBuilder builder,
            IConfigurationRoot configuration,
            IHostEnvironment environment,
            bool optional)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));
            if (environment is null)
                throw new ArgumentNullException(nameof(environment));

            var file = configuration.GetValue<string>("ConfigurationConnectionFile");
            if (!string.IsNullOrEmpty(file))
            {
                if (!File.Exists(file))
                {
                    throw new FileNotFoundException($"The \"ConfigurationConnectionFile\" was not found at: {file}.");
                }

                var section = new ConfigurationBuilder()
                    .SetBasePath(environment.ContentRootPath)
                    .AddJsonFile(file)
                    .Build();

                var connectionOptions =
                    section.Get<ConnectionOptions>() ??
                    throw new NullReferenceException();

                return builder.AddDatabase(connectionOptions);
            }
            else if (configuration.GetSection("ConfigurationConnection") is var section &&
                section.Exists())
            {
                var connectionOptions =
                    section.Get<ConnectionOptions>() ??
                    throw new NullReferenceException($"A \"ConfigurationConnection\" configuration section was expected.");

                return builder.AddDatabase(connectionOptions);
            }
            else if (optional)
            {
                // OK
                return builder;
            }
            else
            {
                throw new NullReferenceException($"A \"ConfigurationConnectionFile\" or \"ConfigurationConnection\" configuration section must be provided.");
            }
        }

        public static IConfigurationBuilder AddDatabase(
            this IConfigurationBuilder builder,
            ConnectionOptions connectionOptions)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (connectionOptions is null)
                throw new ArgumentNullException(nameof(connectionOptions));

            DatabaseConfigurationsSource source = new(connectionOptions);
            return builder.Add(source);
        }
    }
}
