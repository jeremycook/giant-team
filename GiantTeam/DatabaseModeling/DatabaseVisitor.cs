namespace GiantTeam.DatabaseModeling
{
    public class DatabaseVisitor
    {
        private readonly Action<object> visitor;

        public DatabaseVisitor(Action<object> visitor)
        {
            this.visitor = visitor;
        }

        public void Visit(Database database)
        {
            visitor(database);

            foreach (var schemaKVP in database.Schemas)
            {
                var schema = schemaKVP.Value;
                visitor(schemaKVP);
                visitor(schema);
                foreach (var defaultPrivileges in schema.DefaultPrivileges)
                {
                    visitor(defaultPrivileges);
                }
                foreach (var privileges in schema.Privileges)
                {
                    visitor(privileges);
                }
                foreach (var tableKVP in schema.Tables)
                {
                    var table = tableKVP.Value;
                    visitor(tableKVP);
                    visitor(table);
                    foreach (var columnKVP in table.Columns)
                    {
                        var column = columnKVP.Value;
                        visitor(columnKVP);
                        visitor(column);
                    }
                    foreach (var indexKVP in table.Indexes)
                    {
                        var index = indexKVP.Value;
                        visitor(indexKVP);
                        visitor(index);
                    }
                    foreach (var uniqueConstraintKVP in table.UniqueConstraints)
                    {
                        var uniqueConstrain = uniqueConstraintKVP.Value;
                        visitor(uniqueConstraintKVP);
                        visitor(uniqueConstrain);
                    }
                }
            }
        }
    }
}
