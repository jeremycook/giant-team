using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Data;

public class File
{
    [Key]
    public Guid DatumId { get; set; }
    public string ContentType { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
}
