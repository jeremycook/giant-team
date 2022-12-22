using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GiantTeam.DatabaseModeling.Models;

public class Column
{
//    [JsonConstructor]
//#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//    public Column() { }
//#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Column(string name, string storeType, bool isNullable, string? defaultValueSql, string? computedColumnSql)
    {
        Name = name;
        StoreType = storeType;
        IsNullable = isNullable;
        DefaultValueSql = defaultValueSql;
        ComputedColumnSql = computedColumnSql;
    }

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
