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
            return value switch
            {
                Guid guid => guid != Guid.Empty,
                null => false,
                _ => throw new ArgumentException($"The {nameof(value)} argument may only be a {typeof(Guid)} or null, but is a {value.GetType()}.", nameof(value)),
            };
        }
    }
}
