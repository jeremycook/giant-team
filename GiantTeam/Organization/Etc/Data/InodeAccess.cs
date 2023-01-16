using GiantTeam.ComponentModel;
using GiantTeam.Organization.Etc.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

[PrimaryKey(nameof(InodeId), nameof(DbRole))]
public class InodeAccess
{
    [RequiredGuid]
    public Guid InodeId { get; set; }

    [Required]
    public string DbRole { get; set; } = null!;

    /// <summary>
    /// An array of <see cref="PermissionId"/>s.
    /// An empty array indicates that no permissions are granted.
    /// </summary>
    [Required]
    public string[] Permissions { get; set; } = null!;
}
