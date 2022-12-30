using GiantTeam.DatabaseDefinition.Models;

namespace GiantTeam.DatabaseDefinition.Changes.Models
{
    public class CreateColumn : DatabaseChange
    {
        public CreateColumn(string schemaName, string tableName, Column column)
            : base(nameof(CreateColumn))
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