using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.DatabaseDefinition.Models;

public class Table
{
    public Table(string name)
    {
        Name = name;
    }

    [Required, StringLength(50), PgIdentifier]
    public string Name { get; set; }

    [StringLength(50), PgIdentifier]
    public string? Owner { get; set; }

    public List<Column> Columns { get; set; } = new();

    public List<TableIndex> Indexes { get; set; } = new();
}
