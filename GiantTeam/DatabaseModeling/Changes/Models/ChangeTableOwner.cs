namespace GiantTeam.DatabaseModeling.Changes.Models
{
    public class ChangeTableOwner : DatabaseChange
    {
        public ChangeTableOwner(string schemaName, string tableName, string newOwner)
            : base(nameof(ChangeTableOwner))
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