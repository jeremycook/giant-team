using System.Collections.Concurrent;

namespace GiantTeam.DatabaseModel;

public class Schema
{
    public Schema(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public string? Owner { get; set; }
    public ConcurrentDictionary<string, Table> Tables { get; } = new();
    public List<SchemaPrivileges> Privileges { get; } = new();
    public List<DefaultPrivileges> DefaultPrivileges { get; } = new();
}
