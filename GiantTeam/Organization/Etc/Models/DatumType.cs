namespace GiantTeam.Organization.Etc.Models;

public class DatumType
{
    public string TypeId { get; set; } = null!;
    public List<DatumTypeConstraint> Constraints { get; set; } = null!;
}
