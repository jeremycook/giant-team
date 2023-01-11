using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Directory.Data
{
    public class Organization
    {
        [Key]
        [Required, StringLength(50), DatabaseName]
        public string OrganizationId { get; set; } = null!;

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(50), DatabaseName]
        public string DatabaseName { get; set; } = null!;

        public DateTime Created { get; set; }

        public List<OrganizationRole>? Roles { get; set; }
    }
}
