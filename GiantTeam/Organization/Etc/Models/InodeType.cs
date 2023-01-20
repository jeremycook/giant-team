namespace GiantTeam.Organization.Etc.Models;

public class InodeType
{
    public string InodeTypeId { get; init; } = null!;
    public List<string> AllowedChildNodeTypeIds { get; init; } = null!;
    public List<string> AllowedParentNodeTypeIds { get; init; } = null!;
}
