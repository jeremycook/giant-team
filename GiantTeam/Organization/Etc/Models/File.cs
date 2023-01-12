namespace GiantTeam.Organization.Etc.Models;

public class File
{
    public Datum Datum { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
}
