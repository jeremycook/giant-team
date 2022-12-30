namespace GiantTeam.DatabaseDefinition.Changes.Models
{
    public class RenameTable : DatabaseChange
    {
        public RenameTable(string schemaName, string tableName, string newTableName)
            : base(nameof(RenameTable))
        {
            SchemaName = schemaName;
            TableName = tableName;
            NewTableName = newTableName;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public string NewTableName { get; }
    }
}