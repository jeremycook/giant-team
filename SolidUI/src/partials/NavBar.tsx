import { A } from "@solidjs/router";
import { ParentProps, Show } from "solid-js";
import { HomeIcon, HomeOutlineIcon, PersonIcon, PersonOutlineIcon, SparklesIcon, SparklesOutlineIcon, AlertOutlineIcon } from "./Icons";
import { user } from "../utils/session";
import { toggleNotifications } from "./Toasts";

export function Navbar(props: ParentProps) {
    return <>
        <nav class='site-nav bg-gradient-from-gray-800/70  bg-gradient-to-black/80 bg-gradient-to-black/90 bg-gradient-linear rounded-t-2' role='navigation'>
            <Show when={user.isAuthenticated} fallback={<>
                <div class='flex mx-auto children:p-2 children:text-light'>
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
            </>}>
                <div class='grid grid-cols-3'>
                    <div />
                    <div class='flex'>
                        {props.children}
                    </div>
                    <div class='flex'>
                        <div class='ml-auto flex children:p-2 children:text-light'>
                            <A href='/' end={true}>
                                <HomeIcon class='parent-active' />
                                <HomeOutlineIcon class='parent-inactive' />
                                <span class='sr-only'> Home</span>
                            </A>
                            <button onclick={() => toggleNotifications()}>
                                <AlertOutlineIcon />
                                <span class='sr-only'> Notifications</span>
                            </button>
                            <div class='dropdown'>
                                <button class='dropdown-button text-light' type='button' id='site-navbar-user-dropdown'>
                                    <PersonIcon class='parent-active' />
                                    <PersonOutlineIcon class='parent-inactive' />
                                    <span class='sr-only'> Profile</span>
                                </button>
                                <div class='dropdown-anchor' aria-labelledby='site-navbar-user-dropdown'>
                                    <div class='dropdown-content stack bg-dark rounded shadow children:text-light position-right--2'>
                                        <strong class='stack-item'>Hi {user.username}!</strong>
                                        <A href='/my' class='stack-item'>My Profile</A>
                                        <A href='/logout' class='stack-item'>Logout</A>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </Show>
        </nav>
    </>
}