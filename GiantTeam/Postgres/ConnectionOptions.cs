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

        public NpgsqlConnection CreateOpenConnection()
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = ToConnectionStringBuilder();

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (!string.IsNullOrEmpty(CaCertificate))
            {
                connection.ConfigureCaCertificateValidation(CaCertificate);
            }

            connection.Open();

            if (!string.IsNullOrEmpty(SetRole))
            {
                connection.SetRole(SetRole);
            }

            return connection;
        }

        public async Task<NpgsqlConnection> CreateOpenConnectionAsync()
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = ToConnectionStringBuilder();

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (!string.IsNullOrEmpty(CaCertificate))
            {
                connection.ConfigureCaCertificateValidation(CaCertificate);
            }

            await connection.OpenAsync();

            if (!string.IsNullOrEmpty(SetRole))
            {
                await connection.SetRoleAsync(SetRole);
            }

            return connection;
        }

        public NpgsqlConnectionStringBuilder ToConnectionStringBuilder()
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = new(ConnectionString);

            if (!string.IsNullOrEmpty(Username))
            {
                connectionStringBuilder.Username = Username;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                connectionStringBuilder.Password = Password;
            }

            return connectionStringBuilder;
        }
    }
}