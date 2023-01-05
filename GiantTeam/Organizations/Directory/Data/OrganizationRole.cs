using GiantTeam.Organizations.Directory.Helpers;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Directory.Data
{
    public class OrganizationRole
    {
        public OrganizationRole Init()
        {
            OrganizationRoleId = Guid.NewGuid();
            Created = DateTimeOffset.UtcNow;
            DbRole = DirectoryHelpers.OrganizationRole(OrganizationRoleId);
            return this;
        }

        [Key]
        public Guid OrganizationRoleId { get; private set; }

        public DateTimeOffset Created { get; private set; }

        public string OrganizationId { get; private set; } = null!;
        public Organization? Organization { get; private set; }

        [StringLength(50)]
        public string Name { get; set; } = null!;

        [StringLength(60)]
        public string DbRole { get; private set; } = null!;

        public string Description { get; set; } = string.Empty;
    }
}