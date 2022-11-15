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
        private readonly DatabaseConnectionService databaseConnectionService;
        private readonly SessionService sessionService;
        private bool disposedValue;

        public WorkspaceService(
            DatabaseConnectionService databaseConnectionService,
            SessionService sessionService)
        {
            this.databaseConnectionService = databaseConnectionService;
            this.sessionService = sessionService;
        }

        public async Task<Workspace?> GetWorkspaceAsync(string workspaceId)
        {
            var workspaceDbContext = GetWorkspaceDbContext(workspaceId);

            return await workspaceDbContext
                .InformationSchemaTables
                .Where(o => o.table_catalog == workspaceId)
                .Select(o => new Workspace()
                {
                    WorkspaceId = o.table_catalog!,
                    WorkspaceName = o.table_catalog!,
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns a <see cref="WorkspaceDbContext"/> with a lifetime
        /// that is tied to <see cref="WorkspaceService"/>.
        /// Disposal is handled by <see cref="WorkspaceService"/>.
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
