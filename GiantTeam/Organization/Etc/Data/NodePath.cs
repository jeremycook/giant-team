using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Etc.Data;

[Keyless]
public class NodePath
{
    public Guid NodeId { get; private set; }
    public Node? Node { get; private set; }

    public string Path { get; private set; } = null!;
}
