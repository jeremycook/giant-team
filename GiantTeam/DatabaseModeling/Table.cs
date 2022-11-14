using System.Collections.Concurrent;

namespace GiantTeam.DatabaseModeling;

public class Table
{
    public Table(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public string? Owner { get; set; }
    public ConcurrentDictionary<string, Column> Columns { get; } = new();
    public ConcurrentDictionary<string, TableIndex> Indexes { get; } = new();
    public ConcurrentDictionary<string, UniqueConstraint> UniqueConstraints { get; } = new();
}
