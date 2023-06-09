﻿using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services;

public class FetchInodeService
{
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory userDataServiceFactory;

    public FetchInodeService(
        ValidationService validationService,
        UserDataServiceFactory userDataServiceFactory)
    {
        this.validationService = validationService;
        this.userDataServiceFactory = userDataServiceFactory;
    }

    public async Task<Inode> FetchInodeAsync(FetchInodeInput input)
    {
        validationService.Validate(input);

        return await FetchInodeAsync(input.OrganizationId, input.InodeId);
    }

    public async Task<Inode> FetchInodeAsync(Guid organizationId, Guid inodeId)
    {
        var ds = userDataServiceFactory.NewDataService(organizationId);

        var inode = await ds.SingleOrDefaultAsync<InodeRecord>($"WHERE inode_id = {inodeId}");

        if (inode is null)
            throw new NotFoundException($"Inode not found.");

        return Inode.CreateFrom(inode);
    }


    public async Task<Inode> FetchInodeByPathAsync(FetchInodeByPathInput input)
    {
        validationService.Validate(input);

        return await FetchInodeByPathAsync(input.OrganizationId, input.Path);
    }

    public async Task<Inode> FetchInodeByPathAsync(Guid organizationId, string path)
    {
        var ds = userDataServiceFactory.NewDataService(organizationId);

        var inode = await ds.SingleOrDefaultAsync<InodeRecord>($"WHERE path = {path.ToLowerInvariant()}");

        if (inode is null)
            throw new NotFoundException($"Inode not found at {path}.");

        return Inode.CreateFrom(inode);
    }


    public async Task<IReadOnlyList<Inode>> FetchInodeChildrenAsync(FetchInodeChildrenInput input)
    {
        validationService.Validate(input);

        return await FetchInodeChildrenAsync(input.OrganizationId, input.ParentInodeId);
    }

    public async Task<IReadOnlyList<Inode>> FetchInodeChildrenAsync(Guid organizationId, Guid parentInodeId)
    {
        var ds = userDataServiceFactory.NewDataService(organizationId);

        var children = await ds.ListAsync<InodeRecord>($"WHERE parent_inode_id = {parentInodeId} AND parent_inode_id <> inode_id ORDER BY path");

        return children.Select(Inode.CreateFrom).ToArray();
    }


    public async Task<IReadOnlyList<Inode>> FetchInodeListAsync(FetchInodeListInput input)
    {
        validationService.Validate(input);

        return await FetchInodeListAsync(input.OrganizationId, input.Path);
    }

    public async Task<IReadOnlyList<Inode>> FetchInodeListAsync(Guid organizationId, string path)
    {
        var ds = userDataServiceFactory.NewDataService(organizationId);

        // TODO: Limit?
        var inodes = await ds.ListAsync<InodeRecord>($"WHERE path = {path} OR path LIKE {(path == string.Empty ? "%" : (path + "/%"))} ORDER BY path");

        return inodes.Select(Inode.CreateFrom).ToList();
    }
}

public class FetchInodeInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [Required]
    public Guid InodeId { get; set; }
}

public class FetchInodeByPathInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// "", "Space/Folder/Another Folder", and "Space/Folder/Another Folder/file.txt"
    /// are all valid paths. "/", "Space/" and "/Space" are not valid.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string Path { get; set; } = null!;
}

public class FetchInodeChildrenInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [Required]
    public Guid ParentInodeId { get; set; }
}

public class FetchInodeListInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [Required(AllowEmptyStrings = true)]
    public string Path { get; set; } = null!;
}
