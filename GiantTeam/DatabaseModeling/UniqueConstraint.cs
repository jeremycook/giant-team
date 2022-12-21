using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GiantTeam.DatabaseModeling;

public class UniqueConstraint
{
    [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public UniqueConstraint() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public UniqueConstraint(string name, bool isPrimaryKey)
    {
        Name = name;
        IsPrimaryKey = isPrimaryKey;
    }

    [Required, StringLength(50), Identifier]
    public string Name { get; set; }

    public bool IsPrimaryKey { get; set; }

    [Required, MinLength(1)]
    public List<string> Columns { get; set; } = new();
}