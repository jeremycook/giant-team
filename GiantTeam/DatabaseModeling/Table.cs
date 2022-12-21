using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GiantTeam.DatabaseModeling;

public class Table
{
    [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Table() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Table(string name)
    {
        Name = name;
    }

    [Required, StringLength(50), Identifier]
    public string Name { get; set; }

    [StringLength(50), Identifier]
    public string? Owner { get; set; }

    public List<Column> Columns { get; set; } = new();

    public List<TableIndex> Indexes { get; set; } = new();

    public List<UniqueConstraint> UniqueConstraints { get; set; } = new();
}
