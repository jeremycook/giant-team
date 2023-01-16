using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organization.Etc.Data;

public class Inode
{
    private string? _uglyName;

    [Key]
    public Guid InodeId { get; set; }

    public Guid ParentInodeId { get; set; }

    public string InodeTypeId { get; set; } = null!;
    public InodeType? InodeType { get; set; }

    [StringLength(248), InodeName]
    public string Name { get; set; } = null!;

    [StringLength(248), InodeName]
    public string UglyName { get => _uglyName ??= Name?.Replace(' ', '_').ToLowerInvariant()!; set => _uglyName = value; }

    public DateTime Created { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string Path { get; private set; } = null!;

    public List<Inode>? Children { get; set; }
}
