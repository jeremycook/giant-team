using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling.Changes
{
    public class AlterColumn : DatabaseChange
    {
        public AlterColumn(string schemaName, string tableName, Column column, IReadOnlyCollection<AlterColumnModification> modifications)
        {
            if (!modifications.Any())
            {
                throw new ArgumentException($"The {nameof(modifications)} argument must contain at least one value.", nameof(modifications));
            }

            SchemaName = schemaName;
            TableName = tableName;
            Column = column;
            Modifications = modifications;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public Column Column { get; }
        public IReadOnlyCollection<AlterColumnModification> Modifications { get; }
    }
}
