using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GiantTeam.DatabaseDefinition.Models;

public class Database
{
    [StringLength(50), PgIdentifier]
    public string? DefaultSchema { get; set; }

    public List<Schema> Schemas { get; } = new();

    [JsonIgnore]
    public List<string> Scripts { get; } = new();
}
