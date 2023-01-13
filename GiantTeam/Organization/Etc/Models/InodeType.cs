namespace GiantTeam.Organization.Etc.Models;

public class InodeType
{
    public string InodeTypeId { get; set; } = null!;
    public List<InodeTypeConstraint> Constraints { get; set; } = null!;
}
