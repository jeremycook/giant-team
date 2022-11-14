using Npgsql;

namespace GiantTeam.Postgres
{
    public class ConnectionOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string? CaCertificate { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SetRole { get; set; }

        public async Task<NpgsqlConnection> CreateOpenConnectionAsync()
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = ToConnectionStringBuilder();

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (CaCertificate is not null)
            {
                connection.ConfigureCaCertificateValidation(CaCertificate);
            }

            await connection.OpenAsync();

            if (SetRole is not null)
            {
                await connection.SetRoleAsync(SetRole);
            }

            return connection;
        }

        public NpgsqlConnectionStringBuilder ToConnectionStringBuilder()
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = new(ConnectionString);

            if (Username is not null)
            {
                connectionStringBuilder.Username = Username;
            }

            if (Password is not null)
            {
                connectionStringBuilder.Password = Password;
            }

            return connectionStringBuilder;
        }
    }
}