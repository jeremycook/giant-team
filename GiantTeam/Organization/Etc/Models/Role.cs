using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Etc.Models;

public class Role
{
    [Required, StringLength(50)]
    public string RoleId { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public DateTime Created { get; private set; }
}