namespace GiantTeam.Organization.Etc.Models;

public class NodeType
{
    public string TypeId { get; set; } = null!;
    public List<NodeTypeConstraint> Constraints { get; set; } = null!;
}
