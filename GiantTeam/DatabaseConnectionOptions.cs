namespace GiantTeam
{
    public class DatabaseConnectionOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string? Password { get; set; }
        public string? SetRole { get; set; }
        public string? CaCertificate { get; set; }
    }
}
