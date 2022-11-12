namespace GiantTeam.DataProtection
{
    public class DataProtectionOptions
    {
        public GiantTeamOptions.ConnectionOptions Connection { get; } = new();
        public byte[]? ProtectionCertificate { get; set; }
    }
}
