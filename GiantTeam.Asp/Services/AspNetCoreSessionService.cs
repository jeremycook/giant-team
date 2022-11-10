using GiantTeam.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GiantTeam.Asp.Services
{
    public class AspNetCoreSessionService : SessionService
    {
        private readonly IHttpContextAccessor? httpContextAccessor;
        private SessionUser? _user;

        public AspNetCoreSessionService(IHttpContextAccessor? httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override SessionUser User => _user ??= new(GetIdentity());

        private ClaimsIdentity GetIdentity() =>
            httpContextAccessor?.HttpContext?.User.Identity as ClaimsIdentity ??
            throw new InvalidOperationException("Could not find the active ClaimsIdentity.");
    }
}
