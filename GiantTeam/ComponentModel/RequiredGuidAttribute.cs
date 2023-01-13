using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires a non-empty <see cref="Guid"/> value.
    /// </summary>
    public class RequiredGuidAttribute : RequiredAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is Guid guid && guid != Guid.Empty)
            {
                return true;
            }
            else if (value is null)
            {
                return false;
            }
            else
            {
                throw new ArgumentException($"The {nameof(value)} argument may only be a {typeof(Guid)} or null, but is a {value.GetType()}.", nameof(value));
            }
        }
    }
}
