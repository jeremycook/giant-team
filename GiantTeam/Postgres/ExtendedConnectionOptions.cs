namespace GiantTeam.Postgres
{
    public class ExtendedConnectionOptions : BasicConnectionOptions
    {
        public string? Password { get; set; }
        public string? SetRole { get; set; }
    }
}