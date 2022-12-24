namespace GiantTeam.DatabaseModeling.Changes
{
    public class ChangeOwner : DatabaseChange
    {
        public ChangeOwner(string schemaName, string tableName, string newOwner)
        {
            SchemaName = schemaName;
            TableName = tableName;
            NewOwner = newOwner;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public string NewOwner { get; }
    }
}