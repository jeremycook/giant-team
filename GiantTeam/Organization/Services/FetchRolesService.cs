using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;

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

    public async Task<IEnumerable<Role>> FetchInodeAsync(FetchInodeInput input)
    {
        validationService.Validate(input);
        return await FetchInodeAsync(input.OrganizationId);
    }

    public async Task<IEnumerable<Role>> FetchInodeAsync(Guid organizationId)
    {
        var dataService = userDataServiceFactory.NewDataService(organizationId);
        var roles = await dataService.ListAsync<Role>($"ORDER BY name");
        return roles;
    }
}

public class FetchRolesInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }
}
