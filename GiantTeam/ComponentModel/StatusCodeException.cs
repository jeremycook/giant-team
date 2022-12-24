using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    public class StatusCodeException : ValidationException
    {
        public StatusCodeException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public StatusCodeException(int statusCode, string label, string description)
            : this(statusCode, $"{statusCode} {label}: {description}")
        {
        }

        public int StatusCode { get; }
    }
}
