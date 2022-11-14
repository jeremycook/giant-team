using System.Collections.Concurrent;

namespace GiantTeam.DatabaseModeling;

public class Database
{
    public string? DefaultSchema { get; set; }
    public ConcurrentDictionary<string, Schema> Schemas { get; } = new();
    public ConcurrentDictionary<string, string> Scripts { get; } = new();
}
