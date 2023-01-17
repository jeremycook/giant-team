using GiantTeam.Text;
using System.Reflection;

namespace GiantTeam.Postgres;

public abstract class SqlMetadataBase
{
    public abstract string DefaultSchema { get; }

    public virtual string? GetTableSchema(Type tableType)
    {
        // TODO: Check TableAttribute
        return DefaultSchema;
    }

    public virtual string GetTableName(Type tableType)
    {
        // TODO: Check TableAttribute
        return TextTransformers.Snakify(tableType.Name);
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

    public virtual PropertyInfo[] GetColumnProperties(Type type)
    {
        return type
            .GetProperties()
            .Where(p =>
                p.GetSetMethod() != null &&
                (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
            )
            .ToArray();
    }
}
