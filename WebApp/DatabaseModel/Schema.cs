using System.Collections.Concurrent;

namespace WebApp.DatabaseModel;

public class Schema
{
    public Schema(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public ConcurrentDictionary<string, Table> Tables { get; } = new();
}
