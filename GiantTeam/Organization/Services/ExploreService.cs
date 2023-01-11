using GiantTeam.ComponentModel;
using GiantTeam.Organization.Etc.Data;
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

        Node node;
        if (input.Path == "/")
        {
            node = await db.Nodes
                .AsNoTracking()
                .Include(o => o.Children)
                .SingleAsync(o => o.NodeId == NodeId.Root);

            // Remove node from children since it is a child of itself in the database
            node.Children!.Remove(node);
        }
        else
        {
            node = await db.NodePaths
                .Where(np => np.Path == input.Path)
                .AsNoTracking()
                .Select(o => o.Node!)
                .Include(o => o.Children)
                .SingleOrDefaultAsync() ??
                throw new NotFoundException($"\"{input.Path}\" was not found.");
        }

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
    public string Path { get; set; } = null!;
}

public class ExploreResult
{
    public string Path { get; set; }
    public Node Node { get; set; } = null!;
}
