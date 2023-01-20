using GiantTeam.Organization.Etc.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

[PrimaryKey(nameof(InodeId), nameof(RoleId))]
public class InodeAccessRecord
{
    [Obsolete("Runtime only", true)]
    public InodeAccessRecord() { }

    public InodeAccessRecord(DateTime created)
    {
        Created = created;
    }

    [Required]
    public Guid InodeId { get; set; }

    [Required, StringLength(50), RegularExpression("^r:[0-9a-f]{32}$")]
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// An array of <see cref="PermissionId"/>s.
    /// If an empty array is provided then any permissions currently granted to <see cref="RoleId"/> will be revoked
    /// </summary>
    [Required]
    public PermissionId[] Permissions { get; set; } = null!;

    public DateTime Created { get; private set; }
}
