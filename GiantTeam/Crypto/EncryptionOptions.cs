using System.Runtime.Versioning;

namespace GiantTeam.Crypto
{
    [RequiresPreviewFeatures]
    public class EncryptionOptions
    {
        public byte[] Key { get; set; } = null!;
    }
}