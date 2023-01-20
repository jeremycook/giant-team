using GiantTeam.ComponentModel;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Cluster.Directory.Services;

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
        Data.Organization[] organizations = await db.Organizations
            .OrderBy(o => o.Name)
            .ToArrayAsync();

        var output = new FetchOrganizationsOutput()
        {
            Organizations = organizations,
        };

        return output;
    }
}

public class FetchOrganizationsOutput
{
    public IEnumerable<Data.Organization> Organizations { get; set; } = null!;
}
