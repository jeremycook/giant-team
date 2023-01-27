import On from "../../helpers/jsx/On";
import { user, UserEvent } from "../login/user";
import A from "./A"
import Icon from "./Icon"

function toggleNotifications() {

}

export function Navbar() {
    return <On event={[UserEvent.loggedin, UserEvent.loggedout]}>{(e: Event) => {
        if (user.isAuthenticated) {
            return AuthenticatedNavbar();
        }
        else {
            return AnonymousNavbar();
        }
    }}</On>
}

function AuthenticatedNavbar() {
    return <div class='grid cols-3'>
        <div />
        <div />
        <div class='flex'>
            <div class='ml-auto flex children:p-2 children:text-light'>
                <A href='/'>
                    <Icon icon="home-12-regular" />
                    {/* <iconify-icon inline icon="fluent:home-12-filled"></iconify-icon> */}
                    {/* <HomeIcon class='parent-active' />
        <HomeOutlineIcon class='parent-inactive' /> */}
                    <span> Home</span>
                </A>

                <A href='/login'>
                    <Icon icon="home-12-filled" />
                    {/* <iconify-icon inline icon="fluent:home-12-filled"></iconify-icon> */}
                    {/* <HomeIcon class='parent-active' />
        <HomeOutlineIcon class='parent-inactive' /> */}
                    <span> Login</span>
                </A>

                <A href='/logout'>
                    <Icon icon="home-12-filled" />
                    {/* <iconify-icon inline icon="fluent:home-12-filled"></iconify-icon> */}
                    {/* <HomeIcon class='parent-active' />
        <HomeOutlineIcon class='parent-inactive' /> */}
                    <span> Logout</span>
                </A>

                <hr />

                <button onclick={() => toggleNotifications()}>
                    {/* <AlertOutlineIcon /> */}
                    <span class='sr-only'> Notifications</span>
                </button>
                <div class='dropdown'>
                    <button class='dropdown-button text-light' type='button' id='site-navbar-user-dropdown'>
                        {/* <PersonIcon class='parent-active' />
        <PersonOutlineIcon class='parent-inactive' /> */}
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
}

function AnonymousNavbar() {
    return <div class='site-navbar'>
        <A href='/'>
            {/* <HomeIcon class='parent-active' />
        <HomeOutlineIcon class='parent-inactive' /> */}
            {' Home'}
        </A>
        <A href='/login'>
            {/* <PersonIcon class='parent-active' />
        <PersonOutlineIcon class='parent-inactive' /> */}
            {' Login'}
        </A>
        <A href='/join'>
            {/* <SparklesIcon class='parent-active' />
        <SparklesOutlineIcon class='parent-inactive' /> */}
            {' Join'}
        </A>
    </div>
}
