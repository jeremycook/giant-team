using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling.Changes
{
    public class CreateColumn : DatabaseChange
    {
        public CreateColumn(string schemaName, string tableName, Column column)
        {
            SchemaName = schemaName;
            TableName = tableName;
            Column = column;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public Column Column { get; }
    }
}