using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

public class InodeTypeRecord
{
    [Key, Required]
    public string InodeTypeId { get; set; } = null!;
}
