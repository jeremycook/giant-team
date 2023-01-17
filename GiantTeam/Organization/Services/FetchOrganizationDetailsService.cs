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
    private readonly UserDataServiceFactory userDbContextFactory;
    private readonly FetchInodeService exploreService;

    public FetchOrganizationDetailsService(
        ValidationService validationService,
        UserDataServiceFactory userDbContextFactory,
        FetchInodeService exploreService)
    {
        this.validationService = validationService;
        this.userDbContextFactory = userDbContextFactory;
        this.exploreService = exploreService;
    }

    public async Task<FetchOrganizationDetailsResult> FetchOrganizationDetailsAsync(FetchOrganizationDetailsInput input)
    {
        validationService.Validate(input);

        var dataService = userDbContextFactory.NewDataService(input.OrganizationId);

        var org = await dataService.SingleAsync<Etc.Data.Inode>($"WHERE inode_id = {InodeId.Root}");
        var roles = await dataService.ListAsync<Role>($"ORDER BY name");

        var result = new FetchOrganizationDetailsResult()
        {
            OrganizationId = input.OrganizationId,
            Name = org.Name,
            Created = org.Created,
            Roles = roles.ToArray(),
        };
        return result;
    }
}

public class FetchOrganizationDetailsInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }
}

public class FetchOrganizationDetailsResult
{
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = null!;
    public DateTime Created { get; init; }
    public Role[] Roles { get; init; } = null!;
}
