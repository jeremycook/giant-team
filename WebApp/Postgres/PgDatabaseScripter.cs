using System.Text;
using WebApp.DatabaseModel;
using static WebApp.Postgres.PgQuote;

namespace WebApp.Postgres
{
    public class PgDatabaseScripter
    {
        public static PgDatabaseScripter Singleton { get; } = new PgDatabaseScripter();

        public string Script(Database database)
        {
            var script = new StringBuilder();

            // TODO: Sort object creation by dependency graph.

            foreach (var schema in database.Schemas.Values)
            {
                if (schema.Owner is not null)
                {
                    script.AppendLine($"SET ROLE {Identifier(schema.Owner)};");
                }

                // Create missing schema
                // https://www.postgresql.org/docs/current/sql-createschema.html
                script.AppendLine($@"CREATE SCHEMA IF NOT EXISTS {Identifier(schema.Name)};");
                script.AppendLine();

                // Apply privileges
                foreach (var privilege in schema.Privileges)
                {
                    script.AppendLine($@"GRANT {privilege.Privileges} ON SCHEMA {Identifier(schema.Name)} TO {Identifier(privilege.Grantee)};");
                }
                script.AppendLine();

                // Apply default privileges
                foreach (var privilege in schema.DefaultPrivileges)
                {
                    script.AppendLine($@"ALTER DEFAULT PRIVILEGES IN SCHEMA {Identifier(schema.Name)} GRANT {privilege.Privileges} ON {privilege.ObjectType} TO {Identifier(privilege.Grantee)};");
                }
                script.AppendLine();

                foreach (var table in schema.Tables.Values)
                {
                    if (table.Owner is not null)
                    {
                        script.AppendLine($"SET ROLE {Identifier(table.Owner)};");
                    }
                    else if (schema.Owner is not null)
                    {
                        script.AppendLine($"SET ROLE {Identifier(schema.Owner)};");
                    }

                    // Create missing table
                    // https://www.postgresql.org/docs/current/sql-createtable.html
                    IEnumerable<string> tableParts;
                    if (table.UniqueConstraints.Values.FirstOrDefault(uc => uc.IsPrimaryKey) is UniqueConstraint primaryKey)
                    {
                        tableParts =
                            // Columns
                            table.Columns.Values.Select(column =>
                                ScriptColumn(column) +
                                // Assume that integer primary keys are identity columns.
                                // TODO: Drive this from the Database model.
                                (primaryKey.Columns.Contains(column.Name) && column.StoreType == "integer" ? " GENERATED ALWAYS AS IDENTITY" : "")
                            )
                            // Primary key constraint
                            .Append($"CONSTRAINT {Identifier(primaryKey.Name)} PRIMARY KEY ({string.Join(", ", primaryKey.Columns.Select(Identifier))})");
                    }
                    else
                    {
                        tableParts = Enumerable.Empty<string>();
                    }
                    string? tableOwner = table.Owner ?? schema.Owner;
                    script.AppendLine($@"CREATE TABLE IF NOT EXISTS {Identifier(schema.Name, table.Name)} ({string.Join(", ", tableParts)});");
                    script.AppendLine();

                    // Add missing columns
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    foreach (var column in table.Columns.Values)
                    {
                        script.AppendLine($@"ALTER TABLE {Identifier(schema.Name, table.Name)} ADD COLUMN IF NOT EXISTS {ScriptColumn(column)};");
                    }
                    script.AppendLine();

                    // Add missing unique constraints
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    var uniqueConstraints = table.UniqueConstraints.Values.Where(uc => !uc.IsPrimaryKey);
                    foreach (var constraint in uniqueConstraints)
                    {
                        script.AppendLine($@"ALTER TABLE {Identifier(schema.Name, table.Name)} ADD CONSTRAINT IF NOT EXISTS {Identifier(constraint.Name)} {(constraint.IsPrimaryKey ? "PRIMARY KEY" : "UNIQUE")} ({string.Join(", ", constraint.Columns.Select(Identifier))});");
                    }
                    if (uniqueConstraints.Any()) script.AppendLine();

                    // Add missing indexes
                    // https://www.postgresql.org/docs/current/sql-createindex.html
                    foreach (var index in table.Indexes.Values)
                    {
                        script.AppendLine($@"CREATE {(index.IsUnique ? "UNIQUE INDEX" : "INDEX")} IF NOT EXISTS {Identifier(index.Name)} ON {Identifier(schema.Name, table.Name)} ({string.Join(", ", index.Columns.Select(Identifier))});");
                    }
                    if (table.Indexes.Any()) script.AppendLine();
                }

                script.AppendLine();
            }

            // Add scripts.
            foreach (var sql in database.Scripts)
            {
                script.Append(sql.Value);
                script.AppendLine();
            }

            return script.ToString();
        }

        private static string ScriptColumn(Column column)
        {
            return $"{Identifier(column.Name)} {column.StoreType} {(column.IsNullable ? "NULL" : "NOT NULL")}" +
                ScriptDefault(column) +
                (column.ComputedColumnSql is not null ? $" GENERATED ALWAYS AS ({column.ComputedColumnSql}) STORED" : "");
        }

        private static string ScriptDefault(Column column)
        {
            if (column.DefaultValueSql is not null)
            {
                if (column.DefaultValueSql != string.Empty)
                {
                    return $" DEFAULT {column.DefaultValueSql}";
                }
                else
                {
                    return column.StoreType switch
                    {
                        "uuid" => " DEFAULT gen_random_uuid()",
                        _ => string.Empty,
                    };
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
