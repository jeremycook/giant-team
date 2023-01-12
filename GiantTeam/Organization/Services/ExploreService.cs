using GiantTeam.ComponentModel;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;

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

        // TODO
        throw new NotImplementedException();

        //Etc.Models.Node node;
        //if (input.Path == "/")
        //{
        //    node = await db.Nodes
        //        .AsNoTracking()
        //        .Where(o => o.NodeId == NodeId.Root)
        //        .Select(o => new Etc.Models.Node()
        //        {
        //            TypeId = o.TypeId,
        //            NodeId = o.NodeId,
        //            ParentId = o.ParentId,
        //            Name = o.Name,
        //            Created = o.Created,
        //            Path = "/",
        //            Children = o.Children!
        //                .Where(c => c.NodeId != c.ParentId)
        //                .Select(c => new Etc.Models.Node()
        //                {
        //                    TypeId = o.TypeId,
        //                    NodeId = o.NodeId,
        //                    ParentId = o.ParentId,
        //                    Name = o.Name,
        //                    Created = o.Created,
        //                    Path = "/",
        //                    Children = null,
        //                }).ToList()
        //        })
        //        .SingleAsync();

        //    // Remove node from children since it is a child of itself in the database
        //    node.Children!.Remove(node);
        //}
        //else
        //{
        //    node = await db.NodePaths
        //        .Where(np => np.Path == input.Path)
        //        .Select(o => new Etc.Models.Node()
        //        {
        //            TypeId = o.Node.TypeId,
        //            NodeId = o.Node.NodeId,
        //            ParentId = o.Node.ParentId,
        //            Name = o.Node.Name,
        //            Created = o.Node.Created,
        //            Path = o.Path,
        //            Children = o.Node.Children!
        //                .Where(c => c.NodeId != c.ParentId)
        //                .Select(c => new Etc.Models.Node()
        //                {
        //                    TypeId = c.Node.TypeId,
        //                    NodeId = o.Node.NodeId,
        //                    ParentId = o.Node.ParentId,
        //                    Name = o.Node.Name,
        //                    Created = o.Node.Created,
        //                    Path = o.Path,
        //                    Children = null,
        //                }).ToList()
        //        })
        //        .SingleOrDefaultAsync() ??
        //        throw new NotFoundException($"\"{input.Path}\" was not found.");
        //}

        //var result = new ExploreResult()
        //{
        //    Node = node,
        //};
        //return result;
    }
}

public class ExploreInput
{
    public string OrganizationId { get; set; } = null!;
    public string Path { get; set; } = null!;
}

public class ExploreResult
{
    public Etc.Models.Node Node { get; set; } = null!;
}
