using System.Text.Json.Serialization;

namespace GiantTeam.Postgres.Models;

public class TabularData
{
    [JsonPropertyOrder(1)]
    public string[] Columns { get; init; } = null!;
    [JsonPropertyOrder(2)]
    public List<object?[]> Rows { get; init; } = null!;
}
