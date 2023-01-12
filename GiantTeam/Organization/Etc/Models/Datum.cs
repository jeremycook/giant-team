using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

public class Datum
{
    public string TypeId { get; set; } = null!;

    public Guid DatumId { get; set; }

    public Guid ParentId { get; set; }

    [StringLength(248), DatumName]
    public string Name { get; set; } = null!;

    public DateTime Created { get; set; }

    public string Path { get; set; } = null!;

    public List<Datum>? Children { get; set; }
}
