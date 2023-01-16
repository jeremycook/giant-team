using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Services;

public class FetchOrganizationDetailsService
{
    private readonly ValidationService validationService;
    private readonly UserDirectoryDbContextFactory userDirectoryDbContextFactory;
    private readonly FetchInodeService exploreService;

    public FetchOrganizationDetailsService(
        ValidationService validationService,
        UserDirectoryDbContextFactory userDirectoryDbContextFactory,
        FetchInodeService exploreService)
    {
        this.validationService = validationService;
        this.userDirectoryDbContextFactory = userDirectoryDbContextFactory;
        this.exploreService = exploreService;
    }

    public async Task<FetchOrganizationDetailsResult> FetchOrganizationDetailsAsync(FetchOrganizationDetailsInput input)
    {
        validationService.Validate(input);

        using var directoryDb = userDirectoryDbContextFactory.NewDbContext();

        var org = await directoryDb.Organizations
            .Include(o => o.Roles)
            .SingleOrDefaultAsync(o => o.OrganizationId == input.OrganizationId) ??
            throw new NotFoundException($"The \"{input.OrganizationId}\" organization was not found.");

        var result = new FetchOrganizationDetailsResult()
        {
            OrganizationId = org.OrganizationId,
            Name = org.Name,
            DatabaseName = org.DatabaseName,
            Created = org.Created,
            Apps = new FetchOrganizationDetailsApp[]
            {
                //new() { AppId = "Explorer", Name = "Explorer", InodeTypeIds = new[] { "Root", "Space", "Folder" } },
                new() { AppId = "Space", Name = "Space", InodeTypeIds = new[] { "Space" } },
                new() { AppId = "Folder", Name = "Folder", InodeTypeIds = new[] { "Folder" } },
                new() { AppId = "File", Name = "File", InodeTypeIds = new[] { "File" } },
                new() { AppId = "Table", Name = "Table", InodeTypeIds = new[] { "Table" } },
            },
            Roles = org.Roles!.Select(r => new FetchOrganizationDetailsRole()
            {
                Name = r.Name,
                DbRole = r.DbRole,
            }).ToArray(),
        };
        return result;
    }
}

public class FetchOrganizationDetailsInput
{
    public string OrganizationId { get; set; } = null!;
}

public class FetchOrganizationDetailsResult
{
    public string OrganizationId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
    public DateTime Created { get; init; }
    public FetchOrganizationDetailsApp[] Apps { get; init; } = null!;
    public FetchOrganizationDetailsRole[] Roles { get; init; } = null!;
    /// <summary>
    /// The Root Inode.
    /// </summary>
    public Etc.Models.Inode Inode { get; init; } = null!;
}

public class FetchOrganizationDetailsApp
{
    public string AppId { get; init; } = null!;
    public string Name { get; init; } = null!;
    /// <summary>
    /// <see cref="Etc.Models.InodeType.InodeTypeId"/>s this app can handle.
    /// </summary>
    public string[] InodeTypeIds { get; init; } = null!;
}

public class FetchOrganizationDetailsRole
{
    public string Name { get; init; } = null!;
    public string DbRole { get; init; } = null!;
}
