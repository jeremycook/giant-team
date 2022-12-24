using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling.Changes
{
    public class AlterIndex : DatabaseChange
    {
        public AlterIndex(string schemaName, string tableName, TableIndex index)
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
