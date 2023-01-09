using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Spaces.Data
{
    public class Space
    {
        [Key, StringLength(50), DatabaseName]
        public string SpaceId { get; set; } = null!;

        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(50), DatabaseName]
        public string SchemaName { get; set; } = null!;

        public DateTime Created { get; set; }
    }
}