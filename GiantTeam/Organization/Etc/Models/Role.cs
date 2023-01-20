using GiantTeam.Organization.Etc.Data;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

public class Role
{
    [Required, StringLength(50), RegularExpression("^r:[0-9a-f]{32}$")]
    public string RoleId { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public DateTime Created { get; private set; }

    public static Role CreateFrom(RoleRecord record)
    {
        return new()
        {
            RoleId = record.RoleId,
            Name = record.Name,
            Created = record.Created,
        };
    }
}