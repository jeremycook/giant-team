using System.Text.Json;

namespace GiantTeam.Organizations.Organization.Data.Spaces
{
    public class Database
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public JsonDocument Schemas { get; set; } = null!;
    }
}