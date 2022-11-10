using GiantTeam.Services;
using System.Security.Claims;

namespace Tests
{
    public class StubSessionService : SessionService
    {
        private readonly SessionUser sessionUser;

        public StubSessionService(SessionUser sessionUser)
        {
            this.sessionUser = sessionUser;
        }

        public override SessionUser User => sessionUser;
    }
}