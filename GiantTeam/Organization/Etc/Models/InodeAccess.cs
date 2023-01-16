using GiantTeam.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

[PrimaryKey(nameof(InodeId), nameof(DbRole))]
public class InodeAccess
{
    [Required]
    public string DbRole { get; set; } = null!;

    /// <summary>
    /// An array of <see cref="PermissionId"/>s.
    /// An empty array indicates that no permissions are granted.
    /// </summary>
    [Required]
    public char[] Permissions { get; set; } = null!;
}
