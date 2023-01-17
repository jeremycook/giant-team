using Npgsql;

namespace GiantTeam.Postgres
{
    public class PgDataServiceScope : IAsyncDisposable
    {
        private readonly NpgsqlConnection connection;
        private readonly Func<ValueTask> disposeAction;

        public PgDataServiceScope(NpgsqlConnection connection, NpgsqlTransaction transaction, Func<ValueTask> disposeAction)
        {
            this.connection = connection;
            Transaction = transaction;
            this.disposeAction = disposeAction;
        }

        public NpgsqlTransaction Transaction { get; }

        public async ValueTask DisposeAsync()
        {
            await disposeAction();
            await Transaction.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}