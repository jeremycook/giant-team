using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling.Changes
{
    public class CreateTable : DatabaseChange
    {
        public CreateTable(string schemaName, string tableName, IReadOnlyCollection<Column> columns)
        {
            SchemaName = schemaName;
            TableName = tableName;
            Columns = columns;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public IReadOnlyCollection<Column> Columns { get; }
    }
}