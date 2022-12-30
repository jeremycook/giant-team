using GiantTeam.DatabaseDefinition.Models;

namespace GiantTeam.DatabaseDefinition.Changes.Models
{
    public class CreateTable : DatabaseChange
    {
        public CreateTable(string schemaName, string tableName, IReadOnlyCollection<Column> columns)
            : base(nameof(CreateTable))
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