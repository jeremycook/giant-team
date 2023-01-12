using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

public class Node
{
    public Guid NodeId { get; set; }

    public Guid ParentId { get; set; }

    public string Path { get; set; } = null!;

    [StringLength(248), NodeName]
    public string Name { get; set; } = null!;

    public string TypeId { get; set; } = null!;

    public DateTime Created { get; set; }

    public List<Node>? Children { get; set; }
}
