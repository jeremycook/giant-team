namespace GiantTeam.DatabaseDefinition.Changes.Models
{
    public class CreateSchema : DatabaseChange
    {
        public CreateSchema(string schemaName)
            : base(nameof(CreateSchema))
        {
            SchemaName = schemaName;
        }

        public string SchemaName { get; }
    }
}