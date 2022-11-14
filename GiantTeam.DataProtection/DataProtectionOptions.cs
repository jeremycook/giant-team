using GiantTeam.Postgres;

namespace GiantTeam.DataProtection
{
    public class DataProtectionOptions
    {
        public string? DataProtectionCertificate { get; set; }
        public ExtendedConnectionOptions DataProtectionConnection { get; } = new();
    }
}
