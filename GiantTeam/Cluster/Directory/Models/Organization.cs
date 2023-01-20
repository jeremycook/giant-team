using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Directory.Models
{
    public class Organization
    {
        [Key, RequiredGuid]
        public Guid OrganizationId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(50), DatabaseName]
        public string DatabaseName { get; set; } = null!;

        [Required, StringLength(50), RegularExpression("^r:[0-9a-f]{32}$")]
        public string DbOwnerRole { get; set; } = null!;

        public DateTime Created { get; private set; }
    }
}
