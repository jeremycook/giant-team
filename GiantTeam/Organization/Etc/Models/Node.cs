using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

public class Node
{
    public string TypeId { get; set; } = null!;

    public Guid NodeId { get; set; }

    public Guid ParentId { get; set; }

    [StringLength(248), NodeName]
    public string Name { get; set; } = null!;

    public DateTime Created { get; set; }

    public string Path { get; set; } = null!;

    public List<Node>? Children { get; set; }
}
