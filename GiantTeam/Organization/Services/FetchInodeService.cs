using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services;

public class FetchInodeService
{
    private readonly ValidationService validationService;
    private readonly UserDbContextFactory userDbContextFactory;

    public FetchInodeService(
        ValidationService validationService,
        UserDbContextFactory userDbContextFactory)
    {
        this.validationService = validationService;
        this.userDbContextFactory = userDbContextFactory;
    }

    public async Task<Etc.Models.Inode> FetchInodeAsync(FetchInodeInput input)
    {
        validationService.Validate(input);

        return await FetchInodeAsync(input.OrganizationId, input.InodeId);
    }

    public async Task<Etc.Models.Inode> FetchInodeAsync(string organizationId, Guid inodeId)
    {
        return await FetchInodeAsync(organizationId, inodeId, null);
    }

    public async Task<Etc.Models.Inode> FetchInodeByPathAsync(FetchInodeByPathInput input)
    {
        validationService.Validate(input);

        return await FetchInodeByPathAsync(input.OrganizationId, input.Path);
    }

    public async Task<Etc.Models.Inode> FetchInodeByPathAsync(string organizationId, string path)
    {
        return await FetchInodeAsync(organizationId, null, path);
    }

    private async Task<Etc.Models.Inode> FetchInodeAsync(string organizationId, Guid? nodeId, string? path)
    {

        using var db = userDbContextFactory.NewDbContext<EtcDbContext>(organizationId);

        IQueryable<Etc.Data.Inode> query;

        if (nodeId is not null)
            query = db.Inodes.Where(o => o.InodeId == nodeId);
        else if (path is not null)
            query = db.Inodes.Where(o => o.Path == path.ToLower());
        else
            throw new InvalidOperationException($"The nodeId or path must be provided.");

        var inode = await query
            .Select(o => new Etc.Models.Inode()
            {
                InodeTypeId = o.InodeTypeId,
                InodeId = o.InodeId,
                ParentInodeId = o.ParentInodeId,
                Name = o.Name,
                Created = o.Created,
                Path = o.Path,
                ChildrenConstraints = (
                    from con in db.InodeTypeConstraints
                    where con.ParentInodeTypeId == o.InodeTypeId && con.InodeTypeId != con.ParentInodeTypeId
                    orderby con.InodeTypeId
                    select new InodeChildConstraint()
                    {
                        InodeTypeId = con.InodeTypeId,
                    }
                ).ToList(),
                Children = o.Children!
                    .Where(c => c.InodeId != c.ParentInodeId)
                    .Select(c => new Etc.Models.Inode()
                    {
                        InodeTypeId = c.InodeTypeId,
                        InodeId = c.InodeId,
                        ParentInodeId = c.ParentInodeId,
                        Name = c.Name,
                        Created = c.Created,
                        Path = c.Path,
                        ChildrenConstraints = (
                            from con in db.InodeTypeConstraints
                            where con.ParentInodeTypeId == c.InodeTypeId && con.InodeTypeId != con.ParentInodeTypeId
                            orderby con.InodeTypeId
                            select new InodeChildConstraint()
                            {
                                InodeTypeId = con.InodeTypeId,
                            }
                        ).ToList(),
                        Children = null,
                    }).ToList()
            })
            .SingleOrDefaultAsync();

        if (inode is null)
            throw new NotFoundException($"Inode not found.");

        return inode;
    }
}

public class FetchInodeInput
{
    [Required]
    public string OrganizationId { get; set; } = null!;

    [RequiredGuid]
    public Guid InodeId { get; set; }
}

public class FetchInodeByPathInput
{
    [Required]
    public string OrganizationId { get; set; } = null!;

    /// <summary>
    /// "", "Space/Folder/Another Folder", and "Space/Folder/Another Folder/file.txt"
    /// are all valid paths. "/", "Space/" and "/Space" are not valid.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string Path { get; set; } = null!;
}