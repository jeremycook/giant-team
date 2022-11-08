namespace GiantTeam.DataProtection
{
    public class DataProtectionOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string? ConnectionCaCertificate { get; set; }
        public string? SetRole { get; set; }
        public byte[]? ProtectionCertificate { get; set; }
    }
}
