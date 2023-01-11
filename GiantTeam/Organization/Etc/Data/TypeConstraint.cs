using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Etc.Data;

[PrimaryKey(nameof(TypeId), nameof(ParentTypeId))]
public class TypeConstraint
{
    public string TypeId { get; set; } = null!;
    public string ParentTypeId { get; set; } = null!;
}
