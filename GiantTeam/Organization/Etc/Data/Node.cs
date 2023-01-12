using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organization.Etc.Data;

public class Node
{
    [Key]
    public Guid NodeId { get; set; }

    public Guid ParentId { get; set; }

    [StringLength(248), NodeName]
    public string Name { get; set; } = null!;

    public string TypeId { get; set; } = null!;
    public NodeType? Type { get; set; }

    public DateTime Created { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string Path { get; private set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string PathLower { get; private set; } = null!;

    public List<Node>? Children { get; set; }
}
