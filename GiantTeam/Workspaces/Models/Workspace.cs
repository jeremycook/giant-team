using GiantTeam.DatabaseModeling.Models;

namespace GiantTeam.Workspaces.Models
{
    public class Workspace
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public Schema[] Zones { get; set; } = null!;
    }
}
