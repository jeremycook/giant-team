using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

public class File
{
    [Key, RequiredGuid]
    public Guid InodeId { get; set; }

    [Required]
    public string ContentType { get; set; } = null!;

    [Required]
    public byte[] Data { get; set; } = null!;
}
