import On from '../../helpers/jsx/On';
import { user, UserEvent } from '../login/user';
import A from './A'
import Icon from './Icon'

function toggleNotifications() {

}

export function Navbar() {
    return <On
        events={[UserEvent.loggedin, UserEvent.loggedout]}
        class='site-navbar navbar'
    >{(e: Event) => {
        if (user.isAuthenticated) {
            return AuthenticatedNavbar();
        }
        else {
            return AnonymousNavbar();
        }
    }}</On>
}

function AuthenticatedNavbar() {
    return <div class='navbar-trio'>
        <div class='navbar-group' />
        <div class='navbar-group' />
        <div class='navbar-group'>
            <A class='navbar-item' href='/'>
                <Icon icon='home-12-regular' />
                <span class='sr-only'> Home</span>
            </A>

            <button class='navbar-item' onclick={() => toggleNotifications()}>
                <Icon icon='alert-12-regular' />
                <span class='sr-only'> Notifications</span>
            </button>

            <div class='navbar-item dropdown'>
                <button class='dropdown-button' type='button' id='site-navbar-user-dropdown'>
                    <Icon icon='person-12-regular' />
                    <span class='sr-only'> Profile</span>
                </button>
                <div class='dropdown-anchor' aria-labelledby='site-navbar-user-dropdown'>
                    <div class='dropdown-content'>
                        <small>Hi {user.username}!</small>
                        <A href='/my'>My Profile</A>
                        <A href='/logout'>Logout</A>
                    </div>
                </div>
            </div>
        </div>
    </div>
}

function AnonymousNavbar() {
    return <div class='navbar-trio'>
        <div class='navbar-group' />
        <div class='navbar-group'>
            <A class='navbar-item' href='/'>
                <Icon icon='home-12-regular' />
                {' Home'}
            </A>
            <A class='navbar-item' href='/login'>
                <Icon icon='person-12-regular' />
                {' Login'}
            </A>
            <A class='navbar-item' href='/join'>
                <Icon icon='sparkle-16-regular' />
                {' Join'}
            </A>
        </div>
        <div class='navbar-group' />
    </div>
}
