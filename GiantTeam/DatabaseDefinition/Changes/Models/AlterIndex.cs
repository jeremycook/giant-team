using GiantTeam.DatabaseDefinition.Models;

namespace GiantTeam.DatabaseDefinition.Changes.Models
{
    public class AlterIndex : DatabaseChange
    {
        public AlterIndex(string schemaName, string tableName, TableIndex index)
            : base(nameof(AlterIndex))
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
