using GiantTeam.ComponentModel.Models;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    public class InvalidRequestException : ObjectStatusException
    {
        public InvalidRequestException(string message)
            : base(ObjectStatus.InvalidRequest(message))
        {
        }

        public InvalidRequestException(IEnumerable<ValidationResult> validationResults)
            : base(ObjectStatus.InvalidRequest(validationResults))
        {
        }
    }
}
