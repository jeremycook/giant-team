using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.DatabaseModeling;

public class EmbeddedResourcesDatabaseScriptsContributor
{
    public static EmbeddedResourcesDatabaseScriptsContributor Singleton { get; } = new();

    /// <summary>
    /// Contribute *.pgsql embedded resources that share a common namespace with <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    public void Contribute<T>(Database database)
    {
        Type type = typeof(T);
        string prefix = type.Namespace + '.';

        string[] resources = type.Assembly.GetManifestResourceNames();
        foreach (string name in resources)
        {
            if (name.EndsWith(".pgsql", StringComparison.InvariantCultureIgnoreCase) &&
                name.StartsWith(prefix))
            {
                using var stream = type.Assembly.GetManifestResourceStream(name);
                if (stream is not null)
                {
                    using var reader = new StreamReader(stream);
                    string text = reader.ReadToEnd();
                    database.Scripts.Add($"""
-- RESOURCE: {name}
{text}
""");
                }
            }
        }
    }
}
