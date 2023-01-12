using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Etc.Data;

[Keyless]
public class DatumPath
{
    public Guid DatumId { get; private set; }
    public Datum? Datum { get; private set; }

    public string Path { get; private set; } = null!;
}
