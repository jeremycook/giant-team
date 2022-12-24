﻿namespace GiantTeam.DatabaseModeling.Changes
{
    public class DropColumn : DatabaseChange
    {
        public DropColumn(string schemaName, string tableName, string columnName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            ColumnName = columnName;
        }

        public string SchemaName { get; }
        public string TableName { get; }
        public string ColumnName { get; }
    }
}