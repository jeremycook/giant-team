using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Services
{
    [Serializable]
    public class ServiceException : ValidationException
    {
        public ServiceException(IEnumerable<ValidationResult> validationResults)
            : base(string.Join(" ", validationResults.Where(vr => vr.ErrorMessage is not null).Select(vr => vr.ErrorMessage!.TrimEnd('.') + ".")))
        {
            ValidationResults.AddRange(validationResults);
        }

        protected ServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public List<ValidationResult> ValidationResults { get; } = new();
    }
}