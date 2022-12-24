using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel.Models
{
    public class ObjectStatus
    {
        public static ObjectStatus NotFound(string message)
        {
            return new(404, "Not Found", message, new());
        }

        public static ObjectStatus InvalidRequest(string message)
        {
            return InvalidRequest(message, Enumerable.Empty<ObjectStatusDetail>());
        }

        public static ObjectStatus InvalidRequest(string message, IEnumerable<ObjectStatusDetail> details)
        {
            return new(400, "Invalid Request", message, new(details));
        }

        public static ObjectStatus InvalidRequest(IEnumerable<ValidationResult> validationResults)
        {
            var count = validationResults.Count();

            if (count == 0)
            {
                throw new ArgumentException($"The {nameof(validationResults)} argument must contain at least one item.", nameof(validationResults));
            }

            return InvalidRequest(count == 1 ? validationResults.First().ErrorMessage! : $"{validationResults.First().ErrorMessage!.TrimEnd('.')} and {count - 1} other inputs were invalid.", validationResults.Select(vr => new ObjectStatusDetail(vr.ErrorMessage!, vr.MemberNames)));
        }

        public ObjectStatus(int status, string statusText, string message, List<ObjectStatusDetail> details)
        {
            Status = status;
            StatusText = statusText;
            Message = message;
            Details = details;
        }

        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Message { get; set; }
        public List<ObjectStatusDetail> Details { get; }
    }
}
