using Npgsql;

namespace GiantTeam.Postgres
{
    public interface IConnectionOptions
    {
        string ConnectionString { get; }
        string? CaCertificate { get; }
        string? Username { get; }
        string? Password { get; }
        string? SetRole { get; }

        NpgsqlConnectionStringBuilder CreateConnectionStringBuilder();
    }
}