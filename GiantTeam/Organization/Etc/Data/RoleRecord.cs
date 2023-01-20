using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

public class RoleRecord
{
    [Obsolete("Runtime only", true)]
    public RoleRecord() { }

    public RoleRecord(DateTime created)
    {
        Created = created;
    }

    [Required, StringLength(50), RegularExpression("^r:[0-9a-f]{32}$")]
    public string RoleId { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public DateTime Created { get; private set; }
}