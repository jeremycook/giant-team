namespace GiantTeam.Postgres
{
    public class BasicConnectionOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string? CaCertificate { get; set; }
    }
}