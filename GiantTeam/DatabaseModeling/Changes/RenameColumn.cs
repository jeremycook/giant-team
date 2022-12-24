namespace GiantTeam.DatabaseModeling.Changes
{
    public class RenameColumn : DatabaseChange
    {
        public RenameColumn(string schemaName, string tableName, string columnName, string newColumnName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            ColumnName = columnName;
            NewColumnName = newColumnName;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public string ColumnName { get; }
        public string NewColumnName { get; }
    }
}