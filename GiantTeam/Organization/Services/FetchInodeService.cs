using GiantTeam.Organization.Etc.Data;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services;

public class FetchInodeService
{
    private readonly UserDbContextFactory userDbContextFactory;

    public FetchInodeService(
        UserDbContextFactory userDbContextFactory)
    {
        this.userDbContextFactory = userDbContextFactory;
    }

    public async Task<FetchInodeResult> FetchInodeAsync(FetchInodeInput input)
    {
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
                        Children = null,
                    }).ToList()
            })
            .SingleAsync();

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
