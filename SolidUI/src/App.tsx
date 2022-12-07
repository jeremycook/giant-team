import { Icon } from '@iconify-icon/solid';
import chevronDown from '@iconify-icons/ion/chevron-down';
import { Link, useRoutes } from 'solid-app-router';
import { Component, Match, Switch } from 'solid-js';
import { SessionStatus } from './api/GiantTeam.Authentication.Api';
import { routes } from './routes';
import { session } from './session';

const App: Component = () => {
  const Route = useRoutes(routes);

  return (
    <>
      <nav class="site-nav flex-right gap-0 menu d-print-none">
        <Link href="/">
          Home
        </Link>
        <div class="mr-auto dropdown">
          <button class="dropdown-button text-nowrap" type="button" id="site-navbar-create-dropdown">
            New 
            <Icon icon={chevronDown} />
          </button>
          <div class="dropdown-content" aria-labelledby="site-navbar-create-dropdown">
            <div class="card menu">
              <Link href="/create-team">
                Team
              </Link>
              <Link href="/create-workspace">
                Workspace
              </Link>
            </div>
          </div>
        </div>

        <Switch fallback={
          <>
            <Link href="/login">
              Login
            </Link>
            {" • "}
            <Link href="/join">
              Join
            </Link>
          </>
        }>
          <Match when={session().status == SessionStatus.Authenticated}>
            <div class="ml-auto dropdown dropdown-right">
              <button class="dropdown-button" type="button" id="site-navbar-user-dropdown">
                {session().username}
              </button>
              <div class="dropdown-content" aria-labelledby="site-navbar-user-dropdown">
                <div class="card menu">
                  <a href="/logout">Logout</a>
                </div>
              </div>
            </div>
          </Match>
        </Switch>
      </nav>

      <main>
        <Route />
      </main>
    </>
  );
};

export default App;
