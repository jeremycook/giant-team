﻿using GiantTeam.DatabaseDefinition;
using GiantTeam.DatabaseDefinition.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GiantTeam.EntityFramework;

public class EntityFrameworkDatabaseContributor
{
    public static EntityFrameworkDatabaseContributor Singleton { get; } = new();

    public void Contribute(Database database, IModel model)
    {
        var defaultSchema = model.GetDefaultSchema();

        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var schemaGroup in entityType.GetTableMappings()
                .GroupBy(o =>
                    o.Table.Schema ??
                    defaultSchema ??
                    throw new InvalidOperationException("A schema was not provided.")
                ))
            {
                var schema = new Schema(schemaGroup.Key);
                database.Schemas.Add(schema);

                foreach (var tableMapping in schemaGroup)
                {
                    var table = schema.Tables.GetOrAdd(new Table(tableMapping.Table.Name));

                    foreach (var (columnMapping, i) in tableMapping.ColumnMappings.Select((c, i) => (columnMaping: c, i)))
                    {
                        var column = table.Columns.GetOrAdd(new Column(
                            name: columnMapping.Column.Name,
                            storeType: columnMapping.Column.StoreType,
                            isNullable: columnMapping.Column.IsNullable,
                            defaultValueSql: columnMapping.Column.DefaultValueSql,
                            computedColumnSql: columnMapping.Column.ComputedColumnSql)
                        {
                            Position = i + 1,
                        });
                    }

                    foreach (var tableIndex in tableMapping.Table.Indexes)
                    {
                        var index = table.Indexes.GetOrAdd(new TableIndex(tableIndex.Name, tableIndex.IsUnique ? TableIndexType.UniqueConstraint : TableIndexType.Index)
                        {
                            Columns = tableIndex.Columns.OrderBy(o => o.Order).Select(c => c.Name).ToList(),
                        });
                    }

                    foreach (var uniqueConstraint in tableMapping.Table.UniqueConstraints)
                    {
                        var index = table.Indexes.GetOrAdd(new TableIndex(uniqueConstraint.Name, uniqueConstraint.GetIsPrimaryKey() ? TableIndexType.PrimaryKey : TableIndexType.UniqueConstraint)
                        {
                            Columns = uniqueConstraint.Columns.OrderBy(o => o.Order).Select(c => c.Name).ToList(),
                        });
                    }
                }
            }
        }
    }
}
