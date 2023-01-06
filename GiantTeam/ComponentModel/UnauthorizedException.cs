using GiantTeam.ComponentModel.Models;

namespace GiantTeam.ComponentModel
{
    public class UnauthorizedException : ObjectStatusException
    {
        public UnauthorizedException(string message)
            : base(ObjectStatus.Unauthorized(message))
        {
        }
    }
}
