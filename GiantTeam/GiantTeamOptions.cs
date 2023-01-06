using GiantTeam.Postgres;

namespace GiantTeam
{
    public class GiantTeamOptions
    {
        public ConnectionOptions HomeConnection { get; } = new();
        public ConnectionOptions MgmtConnection { get; } = new();
        public ConnectionOptions SecurityConnection { get; } = new();
        public ConnectionOptions DirectoryConnection { get; } = new();
        public ConnectionOptions DirectoryManagerConnection { get; } = new();
        public ConnectionOptions OrganizationConnection { get; } = new();
        public string InfoDatabaseName { get; set; } = null!;
    }
}