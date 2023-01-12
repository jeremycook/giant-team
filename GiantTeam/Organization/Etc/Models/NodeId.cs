namespace GiantTeam.Organization.Etc.Models;

public static class NodeId
{
    public static Guid Root { get; } = Guid.Empty;
    public static Guid Etc { get; } = Guid.Parse("3e544ebc-f30a-471f-a8ec-f9e3ac84f19a");
}
