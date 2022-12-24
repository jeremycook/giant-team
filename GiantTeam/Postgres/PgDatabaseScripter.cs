using GiantTeam.DatabaseModeling.Changes;
using GiantTeam.DatabaseModeling.Models;
using GiantTeam.Text;
using System.ComponentModel.DataAnnotations;
using System.Data;
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
DO $DDL$
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
                script.AppendLine($"CREATE SCHEMA IF NOT EXISTS {Identifier(schema.Name)};");
                script.AppendLine();

                // Apply privileges
                foreach (var privilege in schema.Privileges)
                {
                    script.AppendLine($"GRANT {privilege.Privileges} ON SCHEMA {Identifier(schema.Name)} TO {Identifier(privilege.Grantee)};");
                }
                script.AppendLine();

                // Apply default privileges
                foreach (var privilege in schema.DefaultPrivileges)
                {
                    script.AppendLine($"ALTER DEFAULT PRIVILEGES IN SCHEMA {Identifier(schema.Name)} GRANT {privilege.Privileges} ON {privilege.ObjectType} TO {Identifier(privilege.Grantee)};");
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
                                ScriptAddColumnDefinition(column) +
                                // Assume that integer primary keys are identity columns.
                                // TODO: Drive this from the Database model.
                                (primaryKey.Columns.Contains(column.Name) && column.StoreType == "integer" ? " GENERATED ALWAYS AS IDENTITY" : "")
                            )
                            // Primary key constraint
                            .Append($"CONSTRAINT {Identifier(primaryKey.GetName(table))} PRIMARY KEY ({string.Join(", ", primaryKey.Columns.Select(Identifier))})");
                    }
                    else
                    {
                        tableParts = Enumerable.Empty<string>();
                    }
                    script.AppendLine($"CREATE TABLE IF NOT EXISTS {Identifier(schema.Name, table.Name)} ({string.Join(", ", tableParts)});");
                    script.AppendLine();

                    // Add missing columns
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    foreach (var column in table.Columns)
                    {
                        script.AppendLine($"ALTER TABLE {Identifier(schema.Name, table.Name)} ADD COLUMN IF NOT EXISTS {ScriptAddColumnDefinition(column)};");
                    }
                    script.AppendLine();

                    // Add missing unique constraints
                    // https://www.postgresql.org/docs/current/sql-altertable.html
                    var uniqueConstraints = table.Indexes.Where(uc => uc.IndexType != TableIndexType.Index);
                    foreach (var constraint in uniqueConstraints)
                    {
                        string indexType = constraint.IndexType == TableIndexType.PrimaryKey ? "PRIMARY KEY" : "UNIQUE";
                        script.AppendLine($"""
IF NOT EXISTS (SELECT NULL FROM information_schema.table_constraints WHERE constraint_schema = {Literal(schema.Name)} AND constraint_name = {Literal(constraint.GetName(table))})
THEN
    ALTER TABLE {Identifier(schema.Name, table.Name)} ADD CONSTRAINT {Identifier(constraint.GetName(table))} {indexType} ({string.Join(", ", constraint.Columns.Select(Identifier))});
END IF;
""");
                    }
                    if (uniqueConstraints.Any()) script.AppendLine();

                    // Add missing indexes
                    // https://www.postgresql.org/docs/current/sql-createindex.html
                    foreach (var index in table.Indexes.Where(o => o.IndexType == TableIndexType.Index))
                    {
                        script.AppendLine($"CREATE INDEX IF NOT EXISTS {Identifier(index.GetName(table))} ON {Identifier(schema.Name, table.Name)} ({string.Join(", ", index.Columns.Select(Identifier))});");
                    }
                    if (table.Indexes.Any()) script.AppendLine();
                }

                script.AppendLine();
            }

            script.AppendLine("""
END $DDL$;
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
DO $DDL$
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
                            ScriptAddColumnDefinition(column) +
                            // TODO: Drive this from the Database model. Assuming that integer primary keys are identity columns.
                            (primaryKey?.Columns.Contains(column.Name) == true && column.StoreType == "integer" ? " GENERATED ALWAYS AS IDENTITY" : "")
                        );
                    var constraints = table.Indexes
                        .Where(ix => ix.IndexType != TableIndexType.Index)
                        .Select(constraint => $"CONSTRAINT {Identifier(constraint.GetName(table))} {(constraint.IndexType == TableIndexType.PrimaryKey ? "PRIMARY KEY" : "UNIQUE")} ({string.Join(", ", constraint.Columns.Select(Identifier))})");
                    var tableParts = columns.Concat(constraints);
                    script.AppendLine($"CREATE TABLE {Identifier(schema.Name, table.Name)} ({string.Join(", ", tableParts)});");
                    script.AppendLine();

                    // Add indexes
                    // https://www.postgresql.org/docs/current/sql-createindex.html
                    foreach (var index in table.Indexes.Where(o => o.IndexType == TableIndexType.Index))
                    {
                        script.AppendLine($"CREATE INDEX {Identifier(index.GetName(table))} ON {Identifier(schema.Name, table.Name)} ({string.Join(", ", index.Columns.Select(Identifier))});");
                    }
                    if (table.Indexes.Any()) script.AppendLine();
                }

                script.AppendLine();
            }

            script.AppendLine("""
END $DDL$;
""");

            // Add scripts.
            foreach (var sql in database.Scripts)
            {
                script.Append(sql);
                script.AppendLine();
            }

            return script.ToString();
        }

        private static string ScriptAddColumnDefinition(Column column)
        {
            return $"{Identifier(column.Name)} {column.StoreType} {(column.IsNullable ? "NULL" : "NOT NULL")}" +
                ScriptColumnDefault(column) +
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

        private static string ScriptColumnDefault(Column column)
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
                    return $" DEFAULT ''::{column.StoreType}";
                }
            }

            return string.Empty;
        }

        public string ScriptChanges(List<DatabaseChange> changes)
        {
            var script = new StringBuilder();

            foreach (var change in changes)
            {
                switch (change)
                {
                    case RenameTable renameTable:
                        script.AppendLF(ScriptRenameTable(renameTable));
                        break;
                    case ChangeOwner changeTableOwner:
                        script.AppendLF(ScriptChangeTableOwner(changeTableOwner));
                        break;

                    case CreateColumn addColumn:
                        script.AppendLF(ScriptAddColumn(addColumn));
                        break;
                    case AlterColumn alterColumn:
                        script.AppendLF(ScriptAlterColumn(alterColumn));
                        break;
                    case DropColumn dropColumn:
                        script.AppendLF(ScriptDropColumn(dropColumn));
                        break;

                    case CreateIndex addIndex:
                        script.AppendLF(ScriptAddIndex(addIndex));
                        break;
                    case AlterIndex alterIndex:
                        script.AppendLF(ScriptAlterIndex(alterIndex));
                        break;
                    case DropIndex dropIndex:
                        script.AppendLF(ScriptDropIndex(dropIndex));
                        break;

                    default:
                        throw new NotImplementedException(change.GetType().AssemblyQualifiedName);
                }
            }

            return script.ToString();
        }

        private static string ScriptRenameTable(RenameTable change)
        {
            return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} RENAME TO {Identifier(change.NewTableName)};";
        }

        private static string ScriptChangeTableOwner(ChangeOwner change)
        {
            return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} OWNER TO {Identifier(change.NewOwner)};";
        }


        private static string ScriptAddColumn(CreateColumn change)
        {
            return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} ADD COLUMN {ScriptAddColumnDefinition(change.Column)};";
        }

        private static string ScriptRenameColumn(RenameColumn change)
        {
            return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} RENAME COLUMN {Identifier(change.ColumnName)} TO {Identifier(change.NewColumnName)};";
        }

        private static string ScriptAlterColumn(AlterColumn change)
        {
            var commands = new List<string>();

            if (change.Modifications.Contains(AlterColumnModification.Default))
            {
                if (!string.IsNullOrEmpty(change.Column.DefaultValueSql))
                {
                    commands.Add($"ALTER COLUMN {Identifier(change.Column.Name)} SET DEFAULT ({change.Column.DefaultValueSql})");
                }
                else
                {
                    commands.Add($"ALTER COLUMN {Identifier(change.Column.Name)} DROP DEFAULT");
                }
            }
            if (change.Modifications.Contains(AlterColumnModification.Nullability))
            {
                commands.Add($"ALTER COLUMN {Identifier(change.Column.Name)} {(change.Column.IsNullable ? "DROP NOT NULL" : "SET NOT NULL")}");
            }
            if (change.Modifications.Contains(AlterColumnModification.Type))
            {
                commands.Add($"ALTER COLUMN {Identifier(change.Column.Name)} TYPE {change.Column.StoreType}");
            }
            if (change.Modifications.Contains(AlterColumnModification.Generated))
            {
                if (!string.IsNullOrEmpty(change.Column.ComputedColumnSql))
                {
                    throw new ValidationException("Modifying a generated column is not currently supported because it requires dropping the existing column and then recreating it with the new expression. Please manually drop and recreate the generated column if needed.");
                }
                else
                {
                    commands.Add($"ALTER COLUMN {Identifier(change.Column.Name)} DROP EXPRESSION");
                }
            }

            return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} {string.Join(", ", commands)};";
        }

        private static string ScriptDropColumn(DropColumn change)
        {
            return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} DROP COLUMN {Identifier(change.ColumnName)};";
        }


        private static string ScriptAddIndex(CreateIndex change)
        {
            if (change.Index.IndexType == TableIndexType.Index)
            {
                var index = change.Index;
                return $"CREATE INDEX {Identifier(index.GetName(change.TableName))} ON {Identifier(change.SchemaName, change.TableName)} ({string.Join(", ", index.Columns.Select(Identifier))});";
            }
            else
            {
                var constraint = change.Index;
                string type = constraint.IndexType == TableIndexType.PrimaryKey ? "PRIMARY KEY" : "UNIQUE";
                return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} ADD CONSTRAINT {Identifier(constraint.GetName(change.TableName))} {type} ({string.Join(", ", constraint.Columns.Select(Identifier))});";
            }
        }

        private static string ScriptAlterIndex(AlterIndex change)
        {
            if (change.Index.IndexType == TableIndexType.Index)
            {
                var index = change.Index;
                return
                    $"DROP INDEX {Identifier(change.SchemaName, index.GetName(change.TableName))};\n" +
                    $"CREATE INDEX {Identifier(index.GetName(change.TableName))} ON {Identifier(change.SchemaName, change.TableName)} ({string.Join(", ", index.Columns.Select(Identifier))});";
            }
            else
            {
                var constraint = change.Index;
                string type = constraint.IndexType == TableIndexType.PrimaryKey ? "PRIMARY KEY" : "UNIQUE";
                return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} ALTER CONSTRAINT {Identifier(constraint.GetName(change.TableName))} {type} ({string.Join(", ", constraint.Columns.Select(Identifier))});";
            }
        }

        private static string ScriptDropIndex(DropIndex change)
        {
            if (change.Index.IndexType == TableIndexType.Index)
            {
                var index = change.Index;
                return $"DROP INDEX {Identifier(change.SchemaName, index.GetName(change.TableName))};";
            }
            else
            {
                var constraint = change.Index;
                return $"ALTER TABLE {Identifier(change.SchemaName, change.TableName)} DROP CONSTRAINT {Identifier(constraint.GetName(change.TableName))};";
            }
        }
    }
}
