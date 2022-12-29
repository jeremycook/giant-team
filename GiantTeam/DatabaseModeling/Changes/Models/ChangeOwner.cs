namespace GiantTeam.DatabaseModeling.Changes.Models
{
    public class ChangeOwner : DatabaseChange
    {
        public ChangeOwner(string schemaName, string tableName, string newOwner)
            : base(nameof(ChangeOwner))
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