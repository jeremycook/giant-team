using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.DatabaseModeling.Models;

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

    public int Position { get; set; }

    [Required, StringLength(50), PgIdentifier]
    public string Name { get; set; }

    [Required, StringLength(50), RegularExpression("^[a-zA-Z]+$", ErrorMessage = "The {0} may only contain letters.")]
    public string StoreType { get; set; }

    public bool IsNullable { get; set; }

    [PgExpression]
    public string? DefaultValueSql { get; set; }

    [PgExpression]
    public string? ComputedColumnSql { get; set; }
}
