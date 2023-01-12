using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Services;

public class FetchOrganizationDetailsService
{
    private readonly ValidationService validationService;
    private readonly UserDirectoryDbContextFactory userDirectoryDbContextFactory;
    private readonly ExploreService exploreService;

    public FetchOrganizationDetailsService(
        ValidationService validationService,
        UserDirectoryDbContextFactory userDirectoryDbContextFactory,
        ExploreService exploreService)
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

        var rootResult = await exploreService.ExploreAsync(new()
        {
            OrganizationId = input.OrganizationId,
            Path = "", // Root
        });

        var result = new FetchOrganizationDetailsResult()
        {
            OrganizationId = org.OrganizationId,
            Name = org.Name,
            DatabaseName = org.DatabaseName,
            Created = org.Created,
            Roles = org.Roles!.Select(r => new FetchOrganizationDetailsRole()
            {
                Name = r.Name,
                DbRole = r.DbRole,
            }).ToArray(),
            RootNode = rootResult.Node,
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
    public FetchOrganizationDetailsRole[] Roles { get; init; } = null!;
    public Etc.Models.Node RootNode { get; init; } = null!;
}

public class FetchOrganizationDetailsRole
{
    public string Name { get; init; } = null!;
    public string DbRole { get; init; } = null!;
}
