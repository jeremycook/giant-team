using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Models;

namespace GiantTeam.Organization.Services;

public class FetchOrganizationDetailsService
{
    private readonly ValidationService validationService;
    private readonly FetchInodeService fetchInodeService;
    private readonly FetchRolesService fetchRolesService;

    public FetchOrganizationDetailsService(
        ValidationService validationService,
        FetchInodeService fetchInodeService,
        FetchRolesService fetchRolesService)
    {
        this.validationService = validationService;
        this.fetchInodeService = fetchInodeService;
        this.fetchRolesService = fetchRolesService;
    }

    public async Task<FetchOrganizationDetailsResult> FetchOrganizationDetailsAsync(FetchOrganizationDetailsInput input)
    {
        validationService.Validate(input);

        var rootInode = await fetchInodeService.FetchInodeAsync(input.OrganizationId, InodeId.Root);
        var roles = await fetchRolesService.FetchInodeAsync(input.OrganizationId);

        var result = new FetchOrganizationDetailsResult()
        {
            OrganizationId = input.OrganizationId,
            RootInode = rootInode,
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
    public Inode RootInode { get; init; } = null!;
    public IEnumerable<Role> Roles { get; init; } = null!;
}
