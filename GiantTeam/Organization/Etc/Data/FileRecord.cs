using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

public class FileRecord
{
    [Key, RequiredGuid]
    public Guid InodeId { get; set; }

    [Required]
    public string ContentType { get; set; } = null!;

    [Required]
    public byte[] Data { get; set; } = null!;
}
