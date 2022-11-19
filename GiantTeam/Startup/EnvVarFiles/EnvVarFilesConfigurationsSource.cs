using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace GiantTeam.Startup.EnvVarFiles;

internal class EnvVarFilesConfigurationsSource : IConfigurationSource
{
    private readonly string searchDirectory;
    private readonly string envVarSectionSeparator;

    internal EnvVarFilesConfigurationsSource(
        string searchDirectory,
        string envVarSectionSeparator)
    {
        this.searchDirectory = searchDirectory ?? throw new ArgumentNullException(nameof(searchDirectory));
        this.envVarSectionSeparator = envVarSectionSeparator ?? throw new ArgumentNullException(nameof(envVarSectionSeparator));
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EnvVarFilesConfigurationProvider(
            searchDirectory,
            envVarSectionSeparator,
            new PhysicalFileProvider("/")
        );
    }
}