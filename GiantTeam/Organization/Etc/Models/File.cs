namespace GiantTeam.Organization.Etc.Models;

public class File
{
    public Inode Inode { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
}
