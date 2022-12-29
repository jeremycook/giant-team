using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling.Changes.Models
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