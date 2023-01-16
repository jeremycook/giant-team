using GiantTeam.Cluster.Directory.Data;
using GiantTeam.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Cluster.Directory.Services
{
    public class GetDatabaseNameService
    {
        // TODO: Handle removed organizations.
        // TODO: Convert to LRU cache.
        private static Dictionary<string, string> DumbCache { get; } = new();

        private readonly IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory;

        public GetDatabaseNameService(
           IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory)
        {
            this.managerDirectoryDbContextFactory = managerDirectoryDbContextFactory;
        }

        /// <summary>
        /// Returns the name of an organization's database from the directory.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="System.Data.Common.DbException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public string GetDatabaseName(string organizationId)
        {
            if (string.IsNullOrWhiteSpace(organizationId))
            {
                throw new ArgumentException($"'{nameof(organizationId)}' cannot be null or whitespace.", nameof(organizationId));
            }

            if (!DumbCache.TryGetValue(organizationId, out var databaseName))
            {
                using var managerDirectoryDbContext = managerDirectoryDbContextFactory.CreateDbContext();
                databaseName = managerDirectoryDbContext.Organizations
                    .Where(o => o.OrganizationId == organizationId)
                    .Select(o => o.DatabaseName)
                    .SingleOrDefault()
                    ?? throw new NotFoundException($"The \"{organizationId}\" organization was not found.");

                DumbCache[organizationId] = databaseName;
            }

            return databaseName;
        }
    }
}
