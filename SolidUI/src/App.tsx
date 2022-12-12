import { A, useRoutes } from '@solidjs/router';
import { Component, Match, Switch } from 'solid-js';
import { SessionStatus } from './api/GiantTeam.Authentication.Api';
import { routes } from './routes';
import { session } from './session';
import { useTitle } from './title';
import { HomeIcon, ProfileIcon, SearchIcon } from './utils/icons';

const App: Component = () => {
  const Route = useRoutes(routes);
  useTitle();

  return (
    <>
      <nav class="site-nav menu pt md:flex print:hidden" role="navigation">
        <div class="site-nav-group md:ml-auto">
          <A href="/">
            <HomeIcon /> <span class="md:sr-only">Home</span>
          </A>
        </div>

        <div class="site-nav-group px md:px-0 md:w-50% md:max-w-md">
          <form action='/search' class='flex'>
            <input name='q' class='rounded-l grow' placeholder='Search…' />
            <button type='submit' class='button rounded-0 rounded-r'>
              <SearchIcon />
              <span class="sr-only">Search</span>
            </button>
          </form>
        </div>

        <div class="site-nav-group md:mr-auto">
          <Switch fallback={
            <>
              <A href="/login">Login</A>
              <A href="/join">Join</A>
            </>
          }>
            <Match when={session().status == SessionStatus.Authenticated}>
              <div class="dropdown dropdown-right">
                <button class="dropdown-button" type="button" id="site-navbar-user-dropdown">
                  <ProfileIcon /> <span class="md:sr-only">Profile</span>
                </button>
                <div class="dropdown-content" aria-labelledby="site-navbar-user-dropdown">
                  <div class="menu card">
                    <span>Hi {session().username}!</span>
                    <A href="/profile">My Profile</A>
                    <A href="/logout">Logout</A>
                    <hr />
                    <A href="/workspaces">Workspaces</A>
                    <A href="/workspaces/create">New Workspace</A>
                    <hr />
                    <A href="/teams">Teams</A>
                    <A href="/create-team">New Team</A>
                  </div>
                </div>
              </div>
            </Match>
          </Switch>
        </div>
      </nav>

      <main>
        <Route />
      </main>
    </>
  );
};

export default App;
