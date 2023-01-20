using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

[PrimaryKey(nameof(InodeTypeId), nameof(ParentInodeTypeId))]
public class InodeTypeConstraintRecord
{
    [Required]
    public string InodeTypeId { get; set; } = null!;

    [Required]
    public string ParentInodeTypeId { get; set; } = null!;
}
