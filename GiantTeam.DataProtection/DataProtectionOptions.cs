namespace GiantTeam.DataProtection
{
    public class DataProtectionOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string? ConnectionCaCertificate { get; set; } = null!;
        public byte[]? ProtectionCertificate { get; set; } = null!;
    }
}
