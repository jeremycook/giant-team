using GiantTeam.Postgres;

namespace GiantTeam
{
    public class GiantTeamOptions
    {
        public ConnectionOptions MgmtConnection { get; } = new();
        public ConnectionOptions SecConnection { get; } = new();
        public ConnectionOptions UserConnection { get; } = new();
        public string InfoDatabaseName { get; set; } = null!;
    }
}