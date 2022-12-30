using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.DatabaseDefinition.Models;

public class Schema
{
    public Schema(string name)
    {
        Name = name;
    }

    [Required, StringLength(50), PgIdentifier]
    public string Name { get; set; }

    [StringLength(50), PgIdentifier]
    public string? Owner { get; set; }

    public List<Table> Tables { get; set; } = new();
    public List<SchemaPrivileges> Privileges { get; set; } = new();
    public List<DefaultPrivileges> DefaultPrivileges { get; set; } = new();
}
