using System.Security.Claims;

namespace GiantTeam.Services
{
    public abstract class SessionService
    {
        private SessionUser? user;

        public abstract ClaimsIdentity Identity { get; }

        public virtual SessionUser User => user ??= new(Identity);
    }
}
