import { A } from '@solidjs/router';
import { Show } from 'solid-js';
import { AppRoutes } from './AppRoutes';
import { session, isAuthenticated } from './utils/session';
import { HomeIcon, AlertOutlineIcon, PersonOutlineIcon, PersonIcon, SearchIcon, SparklesIcon, SparklesOutlineIcon, HomeOutlineIcon, MenuOutline, MenuIcon } from './helpers/icons';

export function App() {
  return (
    <>
      <nav class='site-nav bg-gray-100 b-b' role='navigation'>

        <div class='py md:grid md:grid-cols-[1fr_2fr_1fr] print:hidden'>

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
                    <A href='/my' class='stack-item'>My Profile</A>
                    <A href='/logout' class='stack-item'>Logout</A>
                  </div>
                </div>
              </div>

            </div>

          </Show>

        </div>

      </nav>

      <AppRoutes />
    </>
  );
};
