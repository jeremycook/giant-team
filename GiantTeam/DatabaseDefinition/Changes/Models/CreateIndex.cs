using GiantTeam.DatabaseDefinition.Models;

namespace GiantTeam.DatabaseDefinition.Changes.Models
{
    public class CreateIndex : DatabaseChange
    {
        public CreateIndex(string schemaName, string tableName, TableIndex index)
            : base(nameof(CreateIndex))
        {
            SchemaName = schemaName;
            TableName = tableName;
            Index = index;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public TableIndex Index { get; }
    }
}