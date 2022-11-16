using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace GiantTeam.ComponentModel
{
    [Serializable]
    public class DetailedValidationException : ValidationException
    {
        public DetailedValidationException(string message)
            : base(message)
        {
            ValidationResults.Add(new(message));
        }

        public DetailedValidationException(IEnumerable<ValidationResult> validationResults)
            : base(string.Join(" ", validationResults.Where(vr => vr.ErrorMessage is not null).Select(vr => vr.ErrorMessage!.TrimEnd('.') + ".")))
        {
            ValidationResults.AddRange(validationResults);
        }

        protected DetailedValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public List<ValidationResult> ValidationResults { get; } = new();
    }
}