namespace GiantTeam.Postgres
{
    public class PgDatabaseScripterOptions
    {
        /// <summary>
        /// When <c>true</c> schema creation and privilege clauses will be scripted.
        /// When <c>false</c>, the default, the schema is expected to already exist.
        /// </summary>
        public bool CreateSchemaIfNotExists { get; set; } = false;
    }
}