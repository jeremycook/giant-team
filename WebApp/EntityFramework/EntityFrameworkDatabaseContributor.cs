using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using GiantTeam.DatabaseModel;

namespace WebApp.EntityFramework;

public class EntityFrameworkDatabaseContributor
{
    public static EntityFrameworkDatabaseContributor Singleton { get; } = new();

    public void Contribute(Database database, IModel model, string fallbackSchema)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var schemaGroup in entityType.GetTableMappings().GroupBy(o => o.Table.Schema ?? fallbackSchema))
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
                            defaultValueSql: columnMapping.Column.DefaultValueSql,
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
}
