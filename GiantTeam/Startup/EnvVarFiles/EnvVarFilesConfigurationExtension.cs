using Microsoft.Extensions.Configuration;

namespace GiantTeam.Startup.EnvVarFiles;

public static class EnvVarFilesConfigurationExtension
{
    /// <summary>
    /// Adds an environment variable files configuration source to <paramref name="builder"/> where
    /// filenames look like environment variable names and the content of the file is the
    /// like the value of an environment variable.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="searchDirectory"></param>
    /// <param name="envVarSectionSeparator"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddEnvVarFiles(
        this IConfigurationBuilder builder,
        string searchDirectory,
        string envVarSectionSeparator = "__")
    {
        EnvVarFilesConfigurationsSource source = new(searchDirectory, envVarSectionSeparator);
        return builder.Add(source);
    }
}