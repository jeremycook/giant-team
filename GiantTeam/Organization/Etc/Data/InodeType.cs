using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

public class InodeType
{
    [Key]
    public string InodeTypeId { get; set; } = null!;
}
