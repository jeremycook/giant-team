using System.Collections.Concurrent;

namespace GiantTeam.DatabaseModel;

public class Database
{
    public ConcurrentDictionary<string, Schema> Schemas { get; } = new();
    public ConcurrentDictionary<string, string> Scripts { get; } = new();
}
