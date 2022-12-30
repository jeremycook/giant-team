using GiantTeam.DatabaseDefinition.Models;

namespace GiantTeam.DatabaseDefinition;

public class FileDatabaseScriptsContributor
{
    private readonly string? basePath;

    public static FileDatabaseScriptsContributor Singleton { get; } = new(null);

    public FileDatabaseScriptsContributor(string? basePath)
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
            string contents = File.ReadAllText(file);

            database.Scripts.Add($"""
-- FILE: {file}
{contents}
""");
        }
    }
}
