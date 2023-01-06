using GiantTeam.DatabaseDefinition.Models;

namespace GiantTeam.DatabaseDefinition.Alterations.Models
{
    public class DropIndex : DatabaseAlteration
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