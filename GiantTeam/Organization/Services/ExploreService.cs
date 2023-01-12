using GiantTeam.Organization.Etc.Data;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services;

public class ExploreService
{
    private readonly UserDbContextFactory userDbContextFactory;

    public ExploreService(
        UserDbContextFactory userDbContextFactory)
    {
        this.userDbContextFactory = userDbContextFactory;
    }

    public async Task<ExploreResult> ExploreAsync(ExploreInput input)
    {
        using var db = userDbContextFactory.NewDbContext<EtcDbContext>(input.OrganizationId);

        Etc.Models.Node node = await db.Nodes
            .Where(o => o.PathLower == input.Path.ToLower())
            .Select(o => new Etc.Models.Node()
            {
                TypeId = o.TypeId,
                NodeId = o.NodeId,
                ParentId = o.ParentId,
                Name = o.Name,
                Created = o.Created,
                Path = o.Path,
                Children = o.Children!
                    .Where(c => c.NodeId != c.ParentId)
                    .Select(c => new Etc.Models.Node()
                    {
                        TypeId = c.TypeId,
                        NodeId = c.NodeId,
                        ParentId = c.ParentId,
                        Name = c.Name,
                        Created = c.Created,
                        Path = c.Path,
                        Children = null,
                    }).ToList()
            })
            .SingleAsync();

        var result = new ExploreResult()
        {
            Node = node,
        };
        return result;
    }
}

public class ExploreInput
{
    public string OrganizationId { get; set; } = null!;

    /// <summary>
    /// "", "Space/Folder/Another Folder", and "Space/Folder/Another Folder/file.txt"
    /// are all valid paths. "/", "Space/" and "/Space" are not valid.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string Path { get; set; } = string.Empty;
}

public class ExploreResult
{
    public Etc.Models.Node Node { get; set; } = null!;
}
