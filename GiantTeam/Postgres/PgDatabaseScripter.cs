using GiantTeam.DatabaseModeling.Models;
using GiantTeam.Text;
using System.Text;
using static GiantTeam.Postgres.PgQuote;

namespace GiantTeam.Postgres
{
    public class PgDatabaseScripter
    {
        public static PgDatabaseScripter Singleton { get; } = new PgDatabaseScripter();

        /// <summary>
        /// Script missing objects. Useful when developing.
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public string ScriptIfNotExists(Database database)
        {
            var script = new StringBuilder();

            script.AppendLine("""
DO $MIGRATION$
BEGIN

""");

            // TODO: Sort object creation by dependency graph.

            foreach (var schema in database.Schemas)
            {
                if (!string.IsNullOrEmpty(schema.Owner))
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

                foreach (var table in schema.Tables)
                {
                    if (!string.IsNullOrEmpty(table.Owner))
                    {
                        script.AppendLine($"SET ROLE {Identifier(table.Owner)};");
                    }
                    else if (!string.IsNullOrEmpty(schema.Owner))
                    {
                        script.AppendLine($"SET ROLE {Identifier(schema.Owner)};");
                    }

                    // Create missing table
                    // https://www.postgresql.org/docs/current/sql-createtable.html
                    IEnumerable<string> tableParts;
                    if (table.Indexes.FirstOrDefault(uc => uc.IndexType == TableIndexType.PrimaryKey) is TableIndex primaryKey)
                    {
                        tableParts =
                            // Columns
                            table.Columns.Select(column =>
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
                    script.AppendLine($@"CREATE TABLE IF NOT EXISTS {Identifier(schema.Name, table.Name)} ({string.Join(", ", tableParts)});");
                    script.AppendLine();

                    // Add missing columns
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    foreach (var column in table.Columns)
                    {
                        script.AppendLine($@"ALTER TABLE {Identifier(schema.Name, table.Name)} ADD COLUMN IF NOT EXISTS {ScriptColumn(column)};");
                    }
                    script.AppendLine();

                    // Add missing unique constraints
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    var uniqueConstraints = table.Indexes.Where(uc => uc.IndexType != TableIndexType.Index);
                    foreach (var constraint in uniqueConstraints)
                    {
                        string indexType = constraint.IndexType == TableIndexType.PrimaryKey ? "PRIMARY KEY" : "UNIQUE";
                        script.AppendLine($"""
IF NOT EXISTS (SELECT NULL FROM information_schema.table_constraints WHERE constraint_schema = {Literal(schema.Name)} AND constraint_name = {Literal(constraint.Name)})
THEN
    ALTER TABLE {Identifier(schema.Name, table.Name)} ADD CONSTRAINT {Identifier(constraint.Name)} {indexType} ({string.Join(", ", constraint.Columns.Select(Identifier))});
END IF;
""");
                    }
                    if (uniqueConstraints.Any()) script.AppendLine();

                    // Add missing indexes
                    // https://www.postgresql.org/docs/current/sql-createindex.html
                    foreach (var index in table.Indexes.Where(o => o.IndexType == TableIndexType.Index))
                    {
                        script.AppendLine($@"CREATE INDEX IF NOT EXISTS {Identifier(index.Name)} ON {Identifier(schema.Name, table.Name)} ({string.Join(", ", index.Columns.Select(Identifier))});");
                    }
                    if (table.Indexes.Any()) script.AppendLine();
                }

                script.AppendLine();
            }

            script.AppendLine("""
END $MIGRATION$;
""");

            // Add scripts.
            foreach (var sql in database.Scripts)
            {
                script.Append(sql);
                script.AppendLine();
            }

            return script.ToString();
        }

        public string ScriptCreateTables(Database database)
        {
            var script = new StringBuilder();

            script.AppendLine("""
DO $MIGRATION$
BEGIN

""");

            // TODO: Sort object creation by dependency graph.

            foreach (var schema in database.Schemas)
            {
                if (!string.IsNullOrEmpty(schema.Owner))
                {
                    script.AppendLine($"SET ROLE {Identifier(schema.Owner)};");
                }

                foreach (var table in schema.Tables)
                {
                    if (!string.IsNullOrEmpty(table.Owner))
                    {
                        script.AppendLine($"SET ROLE {Identifier(table.Owner)};");
                    }
                    else if (!string.IsNullOrEmpty(schema.Owner))
                    {
                        script.AppendLine($"SET ROLE {Identifier(schema.Owner)};");
                    }

                    // Create missing table
                    // https://www.postgresql.org/docs/current/sql-createtable.html
                    TableIndex? primaryKey = table.Indexes.FirstOrDefault(uc => uc.IndexType == TableIndexType.PrimaryKey);
                    var columns = table.Columns.Select(column =>
                            ScriptColumn(column) +
                            // TODO: Drive this from the Database model. Assuming that integer primary keys are identity columns.
                            (primaryKey?.Columns.Contains(column.Name) == true && column.StoreType == "integer" ? " GENERATED ALWAYS AS IDENTITY" : "")
                        );
                    var constraints = table.Indexes
                        .Where(ix => ix.IndexType != TableIndexType.Index)
                        .Select(constraint => $"CONSTRAINT {Identifier(constraint.Name)} {(constraint.IndexType == TableIndexType.PrimaryKey ? "PRIMARY KEY" : "UNIQUE")} ({string.Join(", ", constraint.Columns.Select(Identifier))})");
                    var tableParts = columns.Concat(constraints);
                    script.AppendLine($@"CREATE TABLE {Identifier(schema.Name, table.Name)} ({string.Join(", ", tableParts)});");
                    script.AppendLine();

                    // Add indexes
                    // https://www.postgresql.org/docs/current/sql-createindex.html
                    foreach (var index in table.Indexes.Where(o => o.IndexType == TableIndexType.Index))
                    {
                        script.AppendLine($@"CREATE INDEX {Identifier(index.Name)} ON {Identifier(schema.Name, table.Name)} ({string.Join(", ", index.Columns.Select(Identifier))});");
                    }
                    if (table.Indexes.Any()) script.AppendLine();
                }

                script.AppendLine();
            }

            script.AppendLine("""
END $MIGRATION$;
""");

            // Add scripts.
            foreach (var sql in database.Scripts)
            {
                script.Append(sql);
                script.AppendLine();
            }

            return script.ToString();
        }

        public string ScriptAlterTables(Database database)
        {
            var script = new StringBuilder();

            script.AppendLine("""
DO $MIGRATION$
BEGIN

""");

            // TODO: Sort object creation by dependency graph.

            foreach (var schema in database.Schemas)
            {
                if (!string.IsNullOrEmpty(schema.Owner))
                {
                    script.AppendLine($"SET ROLE {Identifier(schema.Owner)};");
                }

                foreach (var table in schema.Tables)
                {
                    if (!string.IsNullOrEmpty(table.Owner))
                    {
                        script.AppendLine($"SET ROLE {Identifier(table.Owner)};");
                    }
                    else if (!string.IsNullOrEmpty(schema.Owner))
                    {
                        script.AppendLine($"SET ROLE {Identifier(schema.Owner)};");
                    }

                    // Add missing columns
                    // TODO: Alter existing columns
                    // TODO: Drop removed columns
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    foreach (var column in table.Columns)
                    {
                        script.AppendLine($@"ALTER TABLE {Identifier(schema.Name, table.Name)} ADD COLUMN IF NOT EXISTS {ScriptColumn(column)};");
                    }
                    script.AppendLine();

                    // Add missing unique constraints
                    // TODO: Alter existing unique constraints
                    // TODO: Drop removed unique constraints
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    var uniqueConstraints = table.Indexes.Where(uc => uc.IndexType != TableIndexType.Index);
                    foreach (var constraint in uniqueConstraints)
                    {
                        string indexType = constraint.IndexType == TableIndexType.PrimaryKey ? "PRIMARY KEY" : "UNIQUE";
                        script.AppendLine($"""
IF NOT EXISTS (SELECT NULL FROM information_schema.table_constraints WHERE constraint_schema = {Literal(schema.Name)} AND constraint_name = {Literal(constraint.Name)})
THEN
    ALTER TABLE {Identifier(schema.Name, table.Name)} ADD CONSTRAINT {Identifier(constraint.Name)} {indexType} ({string.Join(", ", constraint.Columns.Select(Identifier))});
END IF;
""");
                    }
                    if (uniqueConstraints.Any()) script.AppendLine();

                    // Add missing indexes
                    // TODO: Alter modified indexes
                    // TODO: Drop removed indexes
                    // https://www.postgresql.org/docs/current/sql-createindex.html
                    foreach (var index in table.Indexes.Where(o => o.IndexType == TableIndexType.Index))
                    {
                        script.AppendLine($@"CREATE INDEX IF NOT EXISTS {Identifier(index.Name)} ON {Identifier(schema.Name, table.Name)} ({string.Join(", ", index.Columns.Select(Identifier))});");
                    }
                    if (table.Indexes.Any()) script.AppendLine();
                }

                script.AppendLine();
            }

            script.AppendLine("""
END $MIGRATION$;
""");

            // Add scripts.
            foreach (var sql in database.Scripts)
            {
                script.Append(sql);
                script.AppendLine();
            }

            return script.ToString();
        }

        private static string ScriptColumn(Column column)
        {
            return $"{Identifier(column.Name)} {column.StoreType} {(column.IsNullable ? "NULL" : "NOT NULL")}" +
                ScriptDefault(column) +
                (!string.IsNullOrEmpty(column.ComputedColumnSql) ? $" GENERATED ALWAYS AS ({column.ComputedColumnSql}) STORED" : "");
        }

        private static readonly Dictionary<string, string> DefaultValueSqlMap = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "boolean", "false" },
            { "integer", "0" },
            { "text", "''" },
            { "timestamp with time zone", "(CURRENT_TIMESTAMP AT TIME ZONE 'UTC')" },
            { "uuid", "gen_random_uuid()" },
        };

        private static string ScriptDefault(Column column)
        {
            if (!string.IsNullOrEmpty(column.DefaultValueSql))
            {
                if (column.DefaultValueSql != string.Empty)
                {
                    return $" DEFAULT {column.DefaultValueSql}";
                }
                else if (DefaultValueSqlMap.TryGetValue(column.StoreType, out var defaultValueSql))
                {
                    return $" DEFAULT {defaultValueSql}";
                }
                else if (RegexPatterns.Alpha.IsMatch(column.StoreType))
                {
                    return $" DEFAULT '':{column.StoreType}";
                }
            }

            return string.Empty;
        }
    }
}
