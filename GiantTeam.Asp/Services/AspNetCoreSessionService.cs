using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GiantTeam.Asp.Services
{
    [Service(ServiceType = typeof(SessionService))]
    public class AspNetCoreSessionService : SessionService
    {
        private readonly IHttpContextAccessor? httpContextAccessor;
        private SessionUser? _user;

        public AspNetCoreSessionService(IHttpContextAccessor? httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override SessionUser User => _user ??= SessionUser.Create(GetIdentity());

        private ClaimsIdentity GetIdentity() =>
            httpContextAccessor?.HttpContext?.User.Identity as ClaimsIdentity ??
            throw new InvalidOperationException("Could not find the active ClaimsIdentity.");
    }
}
