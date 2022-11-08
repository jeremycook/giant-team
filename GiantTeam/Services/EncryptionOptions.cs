using System.Runtime.Versioning;

namespace GiantTeam.Services
{
    [RequiresPreviewFeatures]
    public class EncryptionOptions
    {
        public byte[] Key { get; set; } = null!;
    }
}