namespace WebApp.DatabaseModel
{
    public class ObjectDatabaseScriptsContributor
    {
        private readonly string? basePath;

        public static ObjectDatabaseScriptsContributor Singleton { get; } = new(null);

        public ObjectDatabaseScriptsContributor(string? basePath)
        {
            this.basePath = basePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="objectScriptsDirectory">Relative or absolute path to the directory containing object scripts. Filenames should look like "schema.object.pgsql".</param>
        public void Contribute(Database database, string objectScriptsDirectory)
        {
            string absolute = basePath is not null ?
                Path.GetFullPath(objectScriptsDirectory, basePath) :
                Path.GetFullPath(objectScriptsDirectory);

            foreach (var file in Directory.EnumerateFiles(absolute, "*.pgsql"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string contents = File.ReadAllText(file);

                // Last contribution wins
                database.Scripts[name] = contents;
            }
        }
    }
}
