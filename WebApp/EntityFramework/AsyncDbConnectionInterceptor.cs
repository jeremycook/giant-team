using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace WebApp.EntityFramework
{
    public class AsyncDbConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly Func<DbConnection, ConnectionEndEventData, CancellationToken, Task> onConnectionOpenedAsync;

        public AsyncDbConnectionInterceptor(Func<DbConnection, ConnectionEndEventData, CancellationToken, Task> onConnectionOpenedAsync)
        {
            this.onConnectionOpenedAsync = onConnectionOpenedAsync;
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            await onConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            onConnectionOpenedAsync(connection, eventData, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
