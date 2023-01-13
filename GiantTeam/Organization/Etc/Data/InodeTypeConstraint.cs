using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Etc.Data;

[PrimaryKey(nameof(InodeTypeId), nameof(ParentInodeTypeId))]
public class InodeTypeConstraint
{
    public string InodeTypeId { get; set; } = null!;
    public string ParentInodeTypeId { get; set; } = null!;
}
