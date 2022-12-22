using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.DatabaseModeling.Models;

public class TableIndex
{
    public TableIndex(string name, TableIndexType indexType)
    {
        Name = name;
        IndexType = indexType;
    }

    [Required, StringLength(50), PgIdentifier]
    public string Name { get; set; }

    public TableIndexType IndexType { get; set; }

    [Required, MinLength(1)]
    public List<string> Columns { get; set; } = new();
}
