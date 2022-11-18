using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace GiantTeam.ComponentModel
{
    [Serializable]
    public class DetailedValidationException : ValidationException
    {
        protected DetailedValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public DetailedValidationException(string message)
            : base(message)
        {
            StatusCode = 400;
            ValidationResults.Add(new(message));
        }

        public DetailedValidationException(int statusCode, string message)
            : base(message, null)
        {
            StatusCode = statusCode;
            ValidationResults.Add(new(message));
        }

        public DetailedValidationException(IEnumerable<ValidationResult> validationResults)
            : base(string.Join(" ", validationResults.Where(vr => vr.ErrorMessage is not null).Select(vr => vr.ErrorMessage!.TrimEnd('.', ';', ':') + ".")))
        {
            StatusCode = 400;
            ValidationResults.AddRange(validationResults);
        }

        public DetailedValidationException(ValidationException innerException)
            : base(innerException.Message, innerException)
        {
            StatusCode = 400;
            ValidationResults.Add(innerException.ValidationResult);
        }

        public DetailedValidationException(string? message, Exception? innerException) : base(message, innerException)
        {
            StatusCode = 400;
        }

        public int StatusCode { get; }

        public List<ValidationResult> ValidationResults { get; } = new();
    }
}