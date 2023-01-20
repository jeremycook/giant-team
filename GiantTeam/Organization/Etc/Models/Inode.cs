using GiantTeam.ComponentModel;
using GiantTeam.Organization.Etc.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organization.Etc.Models;

public class Inode
{
    private string? _uglyName;

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

    public DateTime? Created { get; protected set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string Path { get; protected set; } = null!;

    public static Inode CreateFrom(InodeRecord record)
    {
        return new()
        {
            InodeId = record.InodeId,
            ParentInodeId = record.ParentInodeId,
            InodeTypeId = record.InodeTypeId,
            Name = record.Name,
            Created = record.Created,
            UglyName = record.UglyName,
            Path = record.Path ?? throw new NullReferenceException($"The {nameof(record)}.{nameof(record.Path)} property is null."),
        };
    }
}
