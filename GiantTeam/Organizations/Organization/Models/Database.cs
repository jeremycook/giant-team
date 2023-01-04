using GiantTeam.DatabaseDefinition.Models;

namespace GiantTeam.Organizations.Organization.Models
{
    public class Database
    {
        public List<Schema> Schemas { get; } = new();
    }
}