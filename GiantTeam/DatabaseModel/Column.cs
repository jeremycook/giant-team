using System.Data;

namespace GiantTeam.DatabaseModel;

public class Column
{
    public Column(string name, string storeType, bool isNullable, string? defaultValueSql, string? computedColumnSql)
    {
        Name = name;
        StoreType = storeType;
        IsNullable = isNullable;
        DefaultValueSql = defaultValueSql;
        ComputedColumnSql = computedColumnSql;
    }

    public string Name { get; }
    public string StoreType { get; }
    public bool IsNullable { get; }
    public string? DefaultValueSql { get; }
    public string? ComputedColumnSql { get; }
}
