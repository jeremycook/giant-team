using GiantTeam.ComponentModel.Models;

namespace GiantTeam.ComponentModel
{
    public class NotFoundException : ObjectStatusException
    {
        public NotFoundException(string message)
            : base(ObjectStatus.NotFound(message))
        {
        }
    }
}
