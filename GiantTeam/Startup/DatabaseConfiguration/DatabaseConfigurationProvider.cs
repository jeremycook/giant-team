using Dapper;
using GiantTeam.Postgres;
using GiantTeam.Startup.Json;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace GiantTeam.Startup.DatabaseConfiguration
{
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        private readonly ConnectionOptions connectionOptions;

        public DatabaseConfigurationProvider(ConnectionOptions connectionOptions)
        {
            this.connectionOptions = connectionOptions ?? throw new ArgumentNullException(nameof(connectionOptions));
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
        {
            return base.GetChildKeys(earlierKeys, parentPath);
        }

        public override void Load()
        {
            using var connection = connectionOptions.CreateOpenConnection();

            var list = connection.Query<KeyValuePair<string, string>>("""
SELECT key "Key", value "Value"
FROM appsettings
WHERE client = CURRENT_ROLE
ORDER BY key;
""");

            if (!list.Any())
            {
                Console.Error.WriteLine("No configuration was loaded from the database.");
            }

            var keys = new SortedSet<string>();
            foreach (var item in list)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(item.Value));
                var result = JsonConfigurationFileParser.Parse(stream);
                foreach (var pair in result)
                {
                    string key =
                        item.Key +
                        (item.Key != string.Empty && pair.Key != string.Empty ? ":" : string.Empty) +
                        pair.Key;

                    Data[key] = pair.Value;

                    keys.Add(key);
                }
            }

            Console.WriteLine($"Loaded configuration from database for these keys: {string.Join(", ", keys)}");
        }

        public override void Set(string key, string? value)
        {
            base.Set(key, value);
        }

        public override bool TryGet(string key, out string? value)
        {
            return base.TryGet(key, out value);
        }
    }
}