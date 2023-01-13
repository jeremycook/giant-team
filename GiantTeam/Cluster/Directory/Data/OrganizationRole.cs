using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Cluster.Directory.Data
{
    public class OrganizationRole
    {
        private string? _dbRole;

        [Key]
        public Guid OrganizationRoleId { get; set; }

        public DateTime Created { get; set; }

        public string OrganizationId { get; set; } = null!;
        public Organization? Organization { get; private set; }

        [StringLength(50), RoleName]
        public string Name { get; set; } = null!;

        public string Description { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string DbRole { get => _dbRole ??= DirectoryHelpers.OrganizationRole(OrganizationRoleId)!; private set => _dbRole = value; }
    }
}