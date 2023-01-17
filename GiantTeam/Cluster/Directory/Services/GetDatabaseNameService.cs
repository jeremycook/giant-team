using GiantTeam.ComponentModel;

namespace GiantTeam.Cluster.Directory.Services
{
    public class GetDatabaseNameService
    {
        // TODO: Handle removed organizations.
        // TODO: Convert to LRU cache.
        private static readonly Dictionary<Guid, string> _cache = new();

        private readonly DirectoryManagementDataService directoryManagementDataService;

        public GetDatabaseNameService(
           DirectoryManagementDataService directoryManagementDataService)
        {
            this.directoryManagementDataService = directoryManagementDataService;
        }

        /// <summary>
        /// Returns the name of an organization's database from the directory.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="System.Data.Common.DbException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public string GetDatabaseName(Guid organizationId)
        {
            if (organizationId == Guid.Empty)
            {
                throw new ArgumentException($"'{nameof(organizationId)}' cannot be all zeros.", nameof(organizationId));
            }

            if (!_cache.TryGetValue(organizationId, out var databaseName))
            {
                databaseName = directoryManagementDataService
                    .ScalarAsync($"SELECT database_name FROM directory.organization WHERE organization_id = {organizationId}")
                    .GetAwaiter()
                    .GetResult()
                    as string;

                if (databaseName is null)
                {
                    throw new NotFoundException($"The \"{organizationId}\" organization was not found.");
                }

                _cache[organizationId] = databaseName;
            }

            return databaseName;
        }
    }
}
