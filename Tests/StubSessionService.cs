﻿using GiantTeam.Services;

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