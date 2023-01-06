using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GiantTeam.Organization.Data.Spaces
{
    [Keyless]
    public class DatabaseDefinition
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public JsonDocument Schemas { get; set; } = null!;
    }
}