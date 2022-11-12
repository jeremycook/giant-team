namespace GiantTeam
{
    public class GiantTeamOptions
    {
        public class ConnectionOptions
        {
            public string ConnectionString { get; set; } = null!;
            public string? Password { get; set; }
            public string? SetRole { get; set; }
            public string? CaCertificate { get; set; }
        }

        public class UserConnectionOptions
        {
            public string ConnectionString { get; set; } = null!;
            public string? CaCertificate { get; set; }
        }

        public ConnectionOptions DominionConnection { get; } = new();
        public ConnectionOptions WorkspaceAdminConnection { get; } = new();
        public UserConnectionOptions WorkspaceUserConnection { get; } = new();
    }
}