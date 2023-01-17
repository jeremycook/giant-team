using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Directory.Data
{
    public class Organization
    {
        [Key]
        [RequiredGuid]
        public Guid OrganizationId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(50), DatabaseName]
        public string DatabaseName { get; set; } = null!;

        [RequiredGuid]
        public Guid DatabaseOwnerOrganizationRoleId { get; set; }

        public DateTime Created { get; set; }

        public List<OrganizationRole>? Roles { get; set; }
    }
}
