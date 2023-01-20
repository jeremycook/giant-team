using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organization.Etc.Data;

public class InodeRecord
{
    private string? _uglyName;

    [Obsolete("Runtime only", true)]
    public InodeRecord() { }

    public InodeRecord(DateTime created)
    {
        Created = created;
    }

    [Key, Required]
    public Guid InodeId { get; set; }

    [Required]
    public Guid ParentInodeId { get; set; }

    [Required]
    public string InodeTypeId { get; set; } = null!;

    [StringLength(248), InodeName]
    public string Name { get; set; } = null!;

    [StringLength(248), InodeName]
    public string UglyName { get => _uglyName ??= Name?.Replace(' ', '_').ToLowerInvariant()!; set => _uglyName = value; }

    public DateTime Created { get; private set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? Path { get; private set; } = null!;
}
