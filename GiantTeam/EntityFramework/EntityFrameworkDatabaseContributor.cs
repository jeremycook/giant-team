using GiantTeam.DatabaseModeling;
using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GiantTeam.EntityFramework;

public class EntityFrameworkDatabaseContributor
{
    public static EntityFrameworkDatabaseContributor Singleton { get; } = new();

    public void Contribute(Database database, IModel model)
    {
        if (model.GetDefaultSchema() is string defaultSchema)
        {
            // Last wins
            database.DefaultSchema = model.GetDefaultSchema();
        }

        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var schemaGroup in entityType.GetTableMappings().GroupBy(o => o.Table.Schema ?? database.DefaultSchema ?? string.Empty))
            {
                var schema = database.Schemas.GetOrAdd(schemaGroup.Key, key => new Schema(key));
                foreach (var tableMapping in schemaGroup)
                {
                    var table = schema.Tables.GetOrAdd(tableMapping.Table.Name, tableName => new Table(tableName));

                    foreach (var columnMapping in tableMapping.ColumnMappings)
                    {
                        var column = table.Columns.GetOrAdd(columnMapping.Column.Name, columnName => new Column(
                            name: columnName,
                            storeType: columnMapping.Column.StoreType,
                            isNullable: columnMapping.Column.IsNullable,
                            defaultValueSql:
                                columnMapping.Column.DefaultValueSql ??
                                (!columnMapping.Column.IsNullable ? GetDefaultValueSql(columnMapping) : null),
                            computedColumnSql: columnMapping.Column.ComputedColumnSql));
                    }

                    foreach (var tableIndex in tableMapping.Table.Indexes)
                    {
                        var index = table.Indexes.GetOrAdd(tableIndex.Name, indexName => new TableIndex(indexName, tableIndex.IsUnique));
                        index.Columns.AddRange(tableIndex.Columns.Select(c => c.Name));
                    }

                    foreach (var uc in tableMapping.Table.UniqueConstraints)
                    {
                        var uniqueConstraint = table.UniqueConstraints.GetOrAdd(uc.Name, indexName => new UniqueConstraint(indexName, uc.GetIsPrimaryKey()));
                        uniqueConstraint.Columns.AddRange(uc.Columns.Select(c => c.Name));
                    }
                }
            }
        }
    }

    private static readonly Dictionary<Type, string> DefaultValueSqlMap = new()
    {
        { typeof(bool), "false" },
        { typeof(int), "0" },
        { typeof(DateTimeOffset), "CURRENT_TIMESTAMP" },
    };

    private static string? GetDefaultValueSql(IColumnMapping columnMapping)
    {
        return DefaultValueSqlMap.TryGetValue(columnMapping.Property.ClrType, out var defaultValueSql) ?
            defaultValueSql :
            null;
    }
}
