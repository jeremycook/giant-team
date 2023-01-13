using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

public class Inode
{
    public string InodeTypeId { get; set; } = null!;

    public Guid InodeId { get; set; }

    public Guid ParentInodeId { get; set; }

    [StringLength(248), InodeName]
    public string Name { get; set; } = null!;

    public DateTime Created { get; set; }

    public string Path { get; set; } = null!;

    public List<Inode>? Children { get; set; }
}
