using System.Security.Claims;

namespace GiantTeam.Services
{
    public class AspNetCoreSessionService : SessionService
    {
        private readonly IHttpContextAccessor? httpContextAccessor;

        public AspNetCoreSessionService(IHttpContextAccessor? httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override ClaimsIdentity Identity =>
            (httpContextAccessor?.HttpContext?.User.Identity as ClaimsIdentity) ??
            throw new InvalidOperationException("Could not find the active ClaimsIdentity.");
    }
}
