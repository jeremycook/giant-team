using GiantTeam.ComponentModel.Models;

namespace GiantTeam.ComponentModel
{
    public class ForbiddenException : ObjectStatusException
    {
        public ForbiddenException(string message)
            : base(ObjectStatus.Forbidden(message))
        {
        }
    }
}
