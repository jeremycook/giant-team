using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling.Changes.Models
{
    public class DropIndex : DatabaseChange
    {
        public DropIndex(string schemaName, string tableName, TableIndex index)
            : base(nameof(DropIndex))
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