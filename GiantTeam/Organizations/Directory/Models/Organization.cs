using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiantTeam.Organizations.Directory.Models
{
    [Table("organizations")]
    public class Organization
    {
        [Key]
        [Required, StringLength(50), DatabaseName]
        public string OrganizationId { get; set; } = null!;

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(50), DatabaseName]
        public string DatabaseName { get; set; } = null!;

        public DateTimeOffset Created { get; private set; }
    }
}
