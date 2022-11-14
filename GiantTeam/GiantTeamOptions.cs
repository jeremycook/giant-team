using GiantTeam.Postgres;

namespace GiantTeam
{
    public class GiantTeamOptions
    {
        public ExtendedConnectionOptions RecordsManagementConnection { get; } = new();
        public ExtendedConnectionOptions WorkspaceAdministrationConnection { get; } = new();
        public BasicConnectionOptions WorkspaceConnection { get; } = new();
    }
}