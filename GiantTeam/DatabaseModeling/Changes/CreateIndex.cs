using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling.Changes
{
    public class CreateIndex : DatabaseChange
    {
        public CreateIndex(string schemaName, string tableName, TableIndex index)
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