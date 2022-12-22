using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GiantTeam.DatabaseModeling.Models;

public class Schema
{
    [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Schema() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
