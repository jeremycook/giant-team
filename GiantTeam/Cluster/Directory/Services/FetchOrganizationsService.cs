using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Cluster.Directory.Services
{
    public class FetchOrganizationsService
    {
        private readonly UserDirectoryDbContextFactory userDirectoryDbContextFactory;

        public FetchOrganizationsService(
            UserDirectoryDbContextFactory userDirectoryDbContextFactory)
        {
            this.userDirectoryDbContextFactory = userDirectoryDbContextFactory;
        }

        /// <summary>
        /// Returns organizations from the directory that the user has access to.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="System.Data.Common.DbException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<FetchOrganizationsOutput> FetchOrganizationsAsync()
        {
            await using var db = userDirectoryDbContextFactory.NewDbContext();
            var organizations = await db.Organizations
                .Include(o => o.Roles)
                .OrderBy(o => o.Name)
                .ToArrayAsync();

            var output = new FetchOrganizationsOutput()
            {
                Organizations = organizations
                    .Select(o => new FetchOrganizationOutput(o))
                    .ToArray(),
            };

            return output;
        }
    }

    public class FetchOrganizationsOutput
    {
        public FetchOrganizationOutput[] Organizations { get; set; } = null!;
    }
}
