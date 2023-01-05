using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Directory.Data
{
    public class Organization
    {
        public Organization Init()
        {
            Created = DateTimeOffset.UtcNow;
            Roles = new();
            return this;
        }

        [Key]
        [Required, StringLength(50), DatabaseName]
        public string OrganizationId { get; set; } = null!;

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(50), DatabaseName]
        public string DatabaseName { get; set; } = null!;

        public DateTimeOffset Created { get; private set; }

        public List<OrganizationRole>? Roles { get; private set; }
    }
}
