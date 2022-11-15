using GiantTeam.RecordsManagement.Data;
using GiantTeam.Services;
using GiantTeam.WorkspaceInteraction.Data;
using GiantTeam.WorkspaceInteraction.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.RecordsManagement.Services
{
    public class WorkspaceService : IDisposable
    {
        private readonly Dictionary<string, WorkspaceDbContext> cache = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly RecordsManagementDbContext recordsManagementDbContext;
        private readonly WorkspaceConnectionService databaseConnectionService;
        private readonly SessionService sessionService;
        private bool disposedValue;

        public WorkspaceService(
            RecordsManagementDbContext recordsManagementDbContext,
            WorkspaceConnectionService databaseConnectionService,
            SessionService sessionService)
        {
            this.recordsManagementDbContext = recordsManagementDbContext;
            this.databaseConnectionService = databaseConnectionService;
            this.sessionService = sessionService;
        }

        public async Task<Workspace?> GetWorkspaceAsync(string workspaceId)
        {
            var workspace = await recordsManagementDbContext
                .Workspaces
                .FindAsync(workspaceId);

            if (workspace is null)
            {
                return null;
            }

            var workspaceDbContext = GetWorkspaceDbContext(workspaceId);

            var hasAccess = await workspaceDbContext
                .InformationSchemaTables
                .AnyAsync(o => o.table_catalog == workspaceId);

            if (hasAccess)
            {
                return workspace;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a new or cached <see cref="WorkspaceDbContext"/> with a lifetime
        /// is managed and disposed by <c>this</c> <see cref="WorkspaceService"/>.
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        public WorkspaceDbContext GetWorkspaceDbContext(string workspaceId)
        {
            if (!cache.TryGetValue(workspaceId, out var workspace))
            {
                workspace = new WorkspaceDbContext(databaseConnectionService, sessionService, databaseName: workspaceId);
                cache.Add(workspaceId, workspace);
            }

            return workspace;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    foreach (var item in cache.Values)
                    {
                        item.Dispose();
                    }
                    cache.Clear();
                }

                // Set large fields to null

                // Done
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
