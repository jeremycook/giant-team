using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using System.Collections.Immutable;

namespace GiantTeam.Organization.Services;

public class FetchRolesService
{
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory userDataServiceFactory;

    public FetchRolesService(
        ValidationService validationService,
        UserDataServiceFactory userDataServiceFactory)
    {
        this.validationService = validationService;
        this.userDataServiceFactory = userDataServiceFactory;
    }

    public async Task<IReadOnlyList<Role>> FetchRolesAsync(FetchInodeInput input)
    {
        validationService.Validate(input);
        return await FetchRolesAsync(input.OrganizationId);
    }

    public async Task<IReadOnlyList<Role>> FetchRolesAsync(Guid organizationId)
    {
        var dataService = userDataServiceFactory.NewDataService(organizationId);
        var roles = await dataService.ListAsync<RoleRecord>($"ORDER BY name");
        return roles.Select(Role.CreateFrom).ToImmutableArray();
    }
}

public class FetchRolesInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }
}
