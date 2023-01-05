using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GiantTeam.Organizations.Directory.Data
{
    /// <remarks>
    /// Marked internal with <see cref="JsonIgnoreAttribute"/>s to
    /// mitigate against accidental or malicious data exfiltration.
    /// </remarks>
    internal class UserPassword
    {
        [Key]
        [JsonIgnore]
        public Guid UserId { get; set; }
        [JsonIgnore]
        public User? User { get; private set; }

        [Required]
        [JsonIgnore]
        public string PasswordDigest { get; set; } = null!;
    }
}
