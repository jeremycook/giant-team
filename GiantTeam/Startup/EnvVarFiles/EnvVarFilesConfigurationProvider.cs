using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace GiantTeam.Startup.EnvVarFiles;

internal class EnvVarFilesConfigurationProvider : ConfigurationProvider, IDisposable
{
    private const string standardSectionSeparator = ":";

    private readonly string searchDirectory;
    private readonly string envVarSectionSeparator;

    private PhysicalFileProvider? fileProvider;
    private bool disposedValue;

    internal EnvVarFilesConfigurationProvider(
        string searchDirectory,
        string envVarSectionSeparator)
    {
        this.searchDirectory = searchDirectory ?? throw new ArgumentNullException(nameof(searchDirectory));
        this.envVarSectionSeparator = envVarSectionSeparator ?? throw new ArgumentNullException(nameof(envVarSectionSeparator));
    }

    public override void Load()
    {
        if (!Directory.Exists(searchDirectory))
        {
            fileProvider = null;
            return;
        }

        fileProvider ??= new PhysicalFileProvider(searchDirectory);

        foreach (IFileInfo file in fileProvider.GetDirectoryContents("/"))
        {
            if (!file.IsDirectory)
            {
                ProcessFile(file);
            }
        }
    }

    private void ProcessFile(IFileInfo file)
    {
        using (StreamReader reader = new(file.CreateReadStream()))
        {
            string secretKey = file.Name
                .Replace(envVarSectionSeparator, standardSectionSeparator);

            string secretValue = reader.ReadToEnd().TrimEnd('\n', '\r');

            Data.Add(secretKey, secretValue);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                fileProvider?.Dispose();
                fileProvider = null;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}