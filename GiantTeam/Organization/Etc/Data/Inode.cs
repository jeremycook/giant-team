using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organization.Etc.Data;

public class Inode
{
    [Key]
    public Guid InodeId { get; set; }

    public Guid ParentInodeId { get; set; }

    [StringLength(248), InodeName]
    public string Name { get; set; } = null!;

    public string InodeTypeId { get; set; } = null!;
    public InodeType? InodeType { get; set; }

    public DateTime Created { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string Path { get; private set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string PathLower { get; private set; } = null!;

    public List<Inode>? Children { get; set; }
}
