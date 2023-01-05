using GiantTeam.Startup;

namespace GiantTeam.Postgres
{
    [NotAService]
    public class PgDataService : PgDataServiceBase
    {
        private readonly Func<string> connectionString;
        private string? _connectionString;

        public PgDataService(ILogger<PgDataService> logger, Func<string> connectionString)
        {
            Logger = logger;
            this.connectionString = connectionString;
        }

        protected override ILogger Logger { get; }

        protected override string ConnectionString => _connectionString ??= connectionString();
    }
}
