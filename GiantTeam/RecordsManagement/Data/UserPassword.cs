using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GiantTeam.RecordsManagement.Data
{
    /// <remarks>
    /// Marked internal with <see cref="JsonIgnoreAttribute"/>s to
    /// mitigate against accidental or malicious data exfiltration.
    /// </remarks>
    internal class UserPassword
    {
        [JsonIgnore]
        [Key]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [JsonIgnore]
        [Required]
        public string PasswordDigest { get; set; } = null!;
    }
}
