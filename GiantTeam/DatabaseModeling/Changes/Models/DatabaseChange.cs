using System.Text.Json.Serialization;

namespace GiantTeam.DatabaseModeling.Changes.Models
{
    [JsonDerivedType(typeof(AlterColumn), nameof(AlterColumn))]
    [JsonDerivedType(typeof(AlterIndex), nameof(AlterIndex))]
    [JsonDerivedType(typeof(ChangeOwner), nameof(ChangeOwner))]
    [JsonDerivedType(typeof(CreateColumn), nameof(CreateColumn))]
    [JsonDerivedType(typeof(CreateIndex), nameof(CreateIndex))]
    [JsonDerivedType(typeof(CreateSchema), nameof(CreateSchema))]
    [JsonDerivedType(typeof(CreateTable), nameof(CreateTable))]
    [JsonDerivedType(typeof(DropColumn), nameof(DropColumn))]
    [JsonDerivedType(typeof(DropIndex), nameof(DropIndex))]
    [JsonDerivedType(typeof(RenameColumn), nameof(RenameColumn))]
    [JsonDerivedType(typeof(RenameTable), nameof(RenameTable))]
    public abstract class DatabaseChange
    {
        protected DatabaseChange(string type)
        {
            Type = type;
        }

        [JsonPropertyName("$type"), JsonPropertyOrder(-1)]
        public string Type { get; }
    }
}