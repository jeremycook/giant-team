using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organization.Etc.Data;

public class Datum
{
    [Key]
    public Guid DatumId { get; set; }

    public Guid ParentId { get; set; }

    [StringLength(248), DatumName]
    public string Name { get; set; } = null!;

    public string TypeId { get; set; } = null!;
    public DatumType? Type { get; set; }

    public DateTime Created { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string Path { get; private set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string PathLower { get; private set; } = null!;

    public List<Datum>? Children { get; set; }
}
