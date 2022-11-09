import { Component, Match, Switch } from 'solid-js';
import { Link, useRoutes, useLocation } from 'solid-app-router';

import { routes } from './routes';
import { session, sessionSetter } from './session';
import { SessionStatus } from './api/GiantTeam.Authentication.Api';

const App: Component = () => {
  const location = useLocation();
  const Route = useRoutes(routes);

  return (
    <>
      <nav class="bg-gray-200 text-gray-900 px-4">
        <ul class="flex items-center list-none">
          <li class="py-2 px-4">
            <Link href="/" class="no-underline hover:underline">
              Home
            </Link>
          </li>
          <li class="py-2 px-4">
            <Link href="/about" class="no-underline hover:underline">
              About
            </Link>
          </li>
          <li class="py-2 px-4">
            <Link href="/error" class="no-underline hover:underline">
              Error
            </Link>
          </li>

          <li class="text-sm flex items-center space-x-1 ml-auto">
            <span>URL:</span>
            <input
              class="w-75px p-1 bg-white text-sm rounded-lg"
              type="text"
              readOnly
              value={location.pathname}
            />
          </li>
          <li class="py-2 px-4">
            <Switch fallback={
              <Link href="/login" class="no-underline hover:underline">
                Login
              </Link>
            }>
              <Match when={session().status == SessionStatus.Authenticated}>
                <Link href="/logout" class="no-underline hover:underline">
                  Logout {session().name}
                </Link>
              </Match>
            </Switch>
          </li>
        </ul>
      </nav>

      <main>
        <Route />
      </main>
    </>
  );
};

export default App;
