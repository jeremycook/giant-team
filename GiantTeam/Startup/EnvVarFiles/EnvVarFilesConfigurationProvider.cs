using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace GiantTeam.Startup.EnvVarFiles;

internal class EnvVarFilesConfigurationProvider : ConfigurationProvider
{
    private const string standardSectionSeparator = ":";

    private readonly string searchDirectory;
    private readonly string envVarSectionSeparator;
    private readonly IFileProvider fileProvider;

    internal EnvVarFilesConfigurationProvider(
        string searchDirectory,
        string envVarSectionSeparator,
        IFileProvider fileProvider)
    {
        this.searchDirectory = searchDirectory ?? throw new ArgumentNullException(nameof(searchDirectory));
        this.envVarSectionSeparator = envVarSectionSeparator ?? throw new ArgumentNullException(nameof(envVarSectionSeparator));
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
    }

    public override void Load()
    {
        IDirectoryContents directoryContents = fileProvider
            .GetDirectoryContents(searchDirectory);

        if (!directoryContents.Exists)
        {
            return;
        }

        foreach (IFileInfo file in directoryContents)
        {
            ProcessFile(file);
        }
    }

    private void ProcessFile(IFileInfo file)
    {
        if (!file.Exists || file.IsDirectory)
        {
            return;
        }

        using (StreamReader reader = new(file.CreateReadStream()))
        {
            string secretKey = file.Name
                .Replace(envVarSectionSeparator, standardSectionSeparator);

            string secretValue = reader.ReadToEnd().TrimEnd('\n', '\r');

            Data.Add(secretKey, secretValue);
        }
    }
}