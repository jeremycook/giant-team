using GiantTeam.Startup;
using Npgsql;

namespace GiantTeam.Postgres
{
    [IgnoreService]
    public class PgDataService : PgDataServiceBase
    {
        private NpgsqlDataSourceBuilder? _builder;

        protected override ILogger Logger { get; }
        protected override string ConnectionString { get; }

        public PgDataService(
            ILogger logger,
            string connectionString)
        {
            Logger = logger;
            ConnectionString = connectionString;
        }

        public override NpgsqlDataSource AcquireDataSource()
        {
            _builder ??= new NpgsqlDataSourceBuilder(ConnectionString)
                .UseBase64RootCertificateConvention();

            return _builder.Build();
        }
    }
}
