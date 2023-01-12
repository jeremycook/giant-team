import { A } from "@solidjs/router";
import { Show } from "solid-js";
import { HomeIcon, HomeOutlineIcon, PersonIcon, PersonOutlineIcon, SparklesIcon, SparklesOutlineIcon, MenuIcon, MenuOutline, SearchIcon, AlertOutlineIcon, BuildingIcon, BuildingOutlineIcon } from "../helpers/icons";
import { user } from "../utils/session";
import { toggleNotifications } from "./Toasts";

export function NavBar() {
    return <>
        <nav class='site-nav bg-gray-100 b-b' role='navigation'>

            <div class='py md:grid md:grid-cols-[1fr_2fr_1fr] print:hidden'>

                <Show when={user.isAuthenticated} fallback={(
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
                        <A href='/organizations'>
                            <BuildingIcon class='parent-active' />
                            <BuildingOutlineIcon class='parent-inactive' />
                            <span class='md:sr-only'> Organizations</span>
                        </A>
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

                        <button onclick={() => toggleNotifications()}>
                            <AlertOutlineIcon />
                            <span class='md:sr-only'> Notifications</span>
                        </button>
                        <div class='dropdown'>
                            <button class='dropdown-button' type='button' id='site-navbar-user-dropdown'>
                                <PersonIcon class='parent-active' />
                                <PersonOutlineIcon class='parent-inactive' />
                                <span class='md:sr-only'> Profile</span>
                            </button>
                            <div class='dropdown-anchor' aria-labelledby='site-navbar-user-dropdown'>
                                <div class='dropdown-content stack md:position-right'>
                                    <strong class='stack-item'>Hi {user.username}!</strong>
                                    <A href='/my' class='stack-item'>My Profile</A>
                                    <A href='/logout' class='stack-item'>Logout</A>
                                </div>
                            </div>
                        </div>

                    </div>

                </Show>

            </div>

        </nav>
    </>
}