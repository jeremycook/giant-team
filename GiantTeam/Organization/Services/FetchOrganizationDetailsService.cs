using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Models;

namespace GiantTeam.Organization.Services;

public class FetchOrganizationDetailsService
{
    private readonly ValidationService validationService;
    private readonly FetchInodeService fetchInodeService;
    private readonly FetchRolesService fetchRolesService;
    private readonly InodeTypeService inodeTypeService;

    public FetchOrganizationDetailsService(
        ValidationService validationService,
        FetchInodeService fetchInodeService,
        FetchRolesService fetchRolesService,
        InodeTypeService inodeTypeService)
    {
        this.validationService = validationService;
        this.fetchInodeService = fetchInodeService;
        this.fetchRolesService = fetchRolesService;
        this.inodeTypeService = inodeTypeService;
    }

    public async Task<OrganizationDetails> FetchOrganizationDetailsAsync(FetchOrganizationDetailsInput input)
    {
        validationService.Validate(input);

        var rootInode = await fetchInodeService.FetchInodeAsync(input.OrganizationId, InodeId.Root);
        var rootChildren = await fetchInodeService.FetchInodeChildrenAsync(input.OrganizationId, InodeId.Root);
        var roles = await fetchRolesService.FetchInodeAsync(input.OrganizationId);
        var inodeTypes = await inodeTypeService.FetchInodeTypesDictionaryAsync(input.OrganizationId);

        var result = new OrganizationDetails()
        {
            OrganizationId = input.OrganizationId,
            RootInode = rootInode,
            RootChildren = rootChildren,
            Roles = roles,
            InodeTypes = inodeTypes,
        };
        return result;
    }
}

public class FetchOrganizationDetailsInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }
}
