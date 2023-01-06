using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organizations.Organization.Data.Spaces
{
    [Table("spaces")]
    public class Space
    {
        [Key, StringLength(50), DatabaseName]
        public string SpaceId { get; set; } = null!;

        public string Name { get; set; } = null!;

        [StringLength(50), DatabaseName]
        public string SchemaName { get; set; } = null!;

        public DateTime Created { get; set; }
    }
}