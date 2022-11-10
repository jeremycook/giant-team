namespace GiantTeam
{
    public class GiantTeamOptions
    {
        public DatabaseConnectionOptions MainConnection { get; } = new();
        public DatabaseConnectionOptions WorkspaceConnection { get; } = new();
    }
}