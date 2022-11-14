using GiantTeam.Postgres;

namespace GiantTeam
{
    public class GiantTeamOptions
    {
        public ConnectionOptions RecordsManagementConnection { get; } = new();
        public ConnectionOptions WorkspaceAdministrationConnection { get; } = new();
        public ConnectionOptions WorkspaceConnection { get; } = new();
    }
}