using Npgsql;

namespace GiantTeam.Postgres
{
    public class ConnectionOptions : IConnectionOptions
    {
        private string? _caCertificate;
        private string? _username;
        private string? _password;
        private string? _setRole;

        public string ConnectionString { get; set; } = null!;
        public string? CaCertificate { get => _caCertificate; set => _caCertificate = !string.IsNullOrEmpty(value) ? value : null; }
        public string? Username { get => _username; set => _username = !string.IsNullOrEmpty(value) ? value : null; }
        public string? Password { get => _password; set => _password = !string.IsNullOrEmpty(value) ? value : null; }
        public string? SetRole { get => _setRole; set => _setRole = !string.IsNullOrEmpty(value) ? value : null; }

        public ConnectionOptions Clone()
        {
            return new()
            {
                ConnectionString = ConnectionString,
                CaCertificate = CaCertificate,
                Username = Username,
                Password = Password,
                SetRole = SetRole,
            };
        }

        public NpgsqlConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return ToConnectionStringBuilder();
        }

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