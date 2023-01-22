using GiantTeam.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace GiantTeam.Postgres;

public abstract class SqlMetadataBase
{
    public abstract string DefaultSchema { get; }

    public virtual string? GetTableSchema(Type tableType)
    {
        var tableAttribute = tableType.GetCustomAttribute<TableAttribute>();
        return tableAttribute?.Schema ?? DefaultSchema;
    }

    public virtual string GetTableName(Type tableType)
    {
        var tableAttribute = tableType.GetCustomAttribute<TableAttribute>();
        return TextTransformers.Snakify(tableAttribute?.Name ?? tableType.Name);
    }

    public Sql GetFullyQualifiedTableIdentifier(Type tableType)
    {
        string? schemaName = GetTableSchema(tableType);
        string tableName = GetTableName(tableType);

        Sql sql = schemaName != null ?
            Sql.Identifier(schemaName, tableName) :
            Sql.Identifier(TextTransformers.Snakify(tableType.Name));

        return sql;
    }

    public virtual string GetColumnName(PropertyInfo property)
    {
        return TextTransformers.Snakify(property.Name);
    }

    public virtual Sql GetColumnIdentifier(PropertyInfo property)
    {
        return Sql.Identifier(GetColumnName(property));
    }

    public virtual PropertyInfo[] GetSelectProperties(Type type)
    {
        return type
            .GetProperties()
            .Where(MayBeAColumn)
            .ToArray();
    }

    public virtual PropertyInfo[] GetInsertProperties(Type type)
    {
        return type
            .GetProperties()
            .Where(p =>
                p.GetSetMethod() != null &&
                MayBeAColumn(p)
            )
            .ToArray();
    }

    private static bool MayBeAColumn(PropertyInfo p)
    {
        var propertyType = p.PropertyType;

        if (!propertyType.IsClass)
            return true;

        if (propertyType == typeof(string))
            return true;

        if (propertyType.IsArray)
            return true;

        return false;
    }
}
