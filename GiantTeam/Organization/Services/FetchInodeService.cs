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

    /// <summary>
    /// Returns the <see cref="Etc.Data.Inode"/> with its immediate children from
    /// the organization at the provided path.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<FetchInodeResult> FetchInodeAsync(FetchInodeInput input)
    {
        validationService.Validate(input);

        using var db = userDbContextFactory.NewDbContext<EtcDbContext>(input.OrganizationId);

        Etc.Models.Inode inode = await db.Inodes
            .Where(o => o.PathLower == input.Path.ToLower())
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
            .SingleOrDefaultAsync()
            ?? throw new NotFoundException($"\"{input.Path}\" not found in the \"{input.OrganizationId}\" organization.");

        var result = new FetchInodeResult()
        {
            Inode = inode,
        };
        return result;
    }
}

public class FetchInodeInput
{
    public string OrganizationId { get; set; } = null!;

    /// <summary>
    /// "", "Space/Folder/Another Folder", and "Space/Folder/Another Folder/file.txt"
    /// are all valid paths. "/", "Space/" and "/Space" are not valid.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string Path { get; set; } = string.Empty;
}

public class FetchInodeResult
{
    public Etc.Models.Inode Inode { get; set; } = null!;
}
