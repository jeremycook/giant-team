using System.Security.Claims;

namespace WebApp.Services
{
    public class SessionService
    {
        private readonly IHttpContextAccessor? httpContextAccessor;

        private ClaimsIdentity? _identity;
        private SessionUser? _user;

        public SessionService(IHttpContextAccessor? httpContextAccessor = null)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public ClaimsIdentity Identity =>
            (_identity ??= httpContextAccessor?.HttpContext?.User.Identity as ClaimsIdentity) ??
            throw new InvalidOperationException("Could not find the active ClaimsIdentity.");

        public SessionUser User => _user ??= new(Identity);
    }
}
