using GiantTeam.DatabaseDefinition.Models;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Spaces.Data
{
    [Keyless]
    public class DatabaseDefinition
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public Schema[] Schemas { get; set; } = null!;
    }
}