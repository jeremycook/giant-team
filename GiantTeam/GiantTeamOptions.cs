using GiantTeam.Postgres;

namespace GiantTeam
{
    public class GiantTeamOptions
    {
        public ConnectionOptions DirectoryManagerConnection { get; } = new();
        public ConnectionOptions SecurityManagerConnection { get; } = new();
        public string UserConnectionString { get; set; } = null!;
    }
}