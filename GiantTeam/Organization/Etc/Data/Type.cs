using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organization.Etc.Data;

[Table("type")]
public class DatumType
{
    [Key]
    public string TypeId { get; set; } = null!;
}
