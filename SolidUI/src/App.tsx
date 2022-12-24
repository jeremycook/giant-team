import { A } from '@solidjs/router';
import { Component, Show } from 'solid-js';
import AppRoutes from './routes';
import { session, isAuthenticated } from './utils/session';
import { HomeIcon, AlertOutlineIcon, PersonOutlineIcon, PersonIcon, SearchIcon, SparklesIcon, SparklesOutlineIcon, HomeOutlineIcon, MenuOutline, MenuIcon } from './helpers/icons';
import { BreadcrumbTrail } from './utils/nav';

const App: Component = () => {
  return (
    <>
      <nav class='site-nav py md:grid md:grid-cols-[1fr_2fr_1fr] print:hidden bg-gray-100 b-b' role='navigation'>

        <Show when={isAuthenticated()} fallback={(
          <>
            <div />
            <div class='md:flex md:mx-auto children:px-4 children:py-2'>

              <A href='/' end={true}>
                <HomeIcon class='parent-active' />
                <HomeOutlineIcon class='parent-inactive' />
                <span> Home</span>
              </A>
              <A href='/login'>
                <PersonIcon class='parent-active' />
                <PersonOutlineIcon class='parent-inactive' />
                {' Login'}
              </A>
              <A href='/join'>
                <SparklesIcon class='parent-active' />
                <SparklesOutlineIcon class='parent-inactive' />
                {' Join'}
              </A>

            </div>
            <div />
          </>
        )}>

          <div class='flex md:ml-auto children:px-4 children:py-2'>
            <A href='/' end={true}>
              <HomeIcon class='parent-active' />
              <HomeOutlineIcon class='parent-inactive' />
              <span class='md:sr-only'> Home</span>
            </A>
            <div class='dropdown'>
              <button class='dropdown-button' type='button' id='site-navbar-menu-dropdown'>
                <MenuIcon class='parent-active' />
                <MenuOutline class='parent-inactive' />
                <span class='md:sr-only'> Menu</span>
              </button>
              <div class='dropdown-anchor' aria-labelledby='site-navbar-menu-dropdown'>
                <div class='dropdown-content stack'>
                  <A href='/workspaces' class='stack-item'>Workspaces</A>
                  <A href='/workspaces/create-workspace' class='stack-item'>New Workspace</A>
                  <hr class='m-0' />
                  <A href='/teams' class='stack-item'>Teams</A>
                  <A href='/create-team' class='stack-item'>New Team</A>
                </div>
              </div>
            </div>
          </div>

          <div class='px md:px-0'>

            <form action='/search' class='flex'>
              <input name='q' class='rounded-l grow' placeholder='Search…' />
              <button type='submit' class='button rounded-0 rounded-r'>
                <SearchIcon />
                <span class='sr-only'>Search</span>
              </button>
            </form>

          </div>

          <div class='flex md:mr-auto children:px-4 children:py-2'>

            <A href='/notifications'>
              <AlertOutlineIcon />
              <span class='md:sr-only'> Notifications</span>
            </A>
            <div class='dropdown'>
              <button class='dropdown-button' type='button' id='site-navbar-user-dropdown'>
                <PersonIcon class='parent-active' />
                <PersonOutlineIcon class='parent-inactive' />
                <span class='md:sr-only'> Profile</span>
              </button>
              <div class='dropdown-anchor' aria-labelledby='site-navbar-user-dropdown'>
                <div class='dropdown-content stack md:position-right'>
                  <strong class='stack-item'>Hi {session.username}!</strong>
                  <A href='/profile' class='stack-item'>My Profile</A>
                  <A href='/logout' class='stack-item'>Logout</A>
                </div>
              </div>
            </div>

          </div>

        </Show>

      </nav>

      <div class='flex gap-2 children:button'>
        <BreadcrumbTrail />
      </div>

      <main>
        <AppRoutes />
      </main>

      <div id='main-portal' class='absolute top-0 left-0 right-0 h-0 flex justify-center'></div>
    </>
  );
};

export default App;
