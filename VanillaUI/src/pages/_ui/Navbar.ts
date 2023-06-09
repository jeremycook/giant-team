import { h } from '../../helpers/h';
import { User } from '../login/user';
import Icon from './Icon'

function toggleNotifications() {

}

export function SiteNavbarUI(user: User) {
    return user.pipe.project(u => u.isAuthenticated
        ? AuthenticatedNavbar(user)
        : AnonymousNavbar())
}

function AuthenticatedNavbar(user: User) {
    return h('.navbar',
        h('.navbar-group'),
        h('.navbar-group'),
        h('.navbar-group',
            h('a.navbar-item', x => x.set({ href: '/' }),
                Icon('home-12-regular'),
                h('span.sr-only', 'Home'),
            ),
            h('button.navbar-item', x => x.set({ onclick: _ => toggleNotifications() }),
                Icon('alert-12-regular'),
                h('span.sr-only', 'Notifications'),
            ),
            h('.navbar-item.dropdown',
                h('button', x => x.set({ type: 'button', id: 'site-navbar-user-dropdown' }),
                    Icon('person-12-regular'),
                    h('span.sr-only', 'Profile'),
                ),
                h('.dropdown-anchor', x => x.set({ 'aria-labelledby': 'site-navbar-user-dropdown' }),
                    h('.dropdown-content',
                        h('small', `Hi ${user.username}`),
                        h('a', x => x.set({ href: '/my' }), 'My Profile'),
                        h('a', x => x.set({ href: '/logout' }), 'Logout'),
                    ),
                )
            )
        ),
    );
}

function AnonymousNavbar() {
    return h('.navbar-trio',
        h('.navbar-group'),
        h('.navbar-group',
            h('a.navbar-item', x => x.set({ href: '/' }),
                Icon('home-12-regular'), ' Home',
            ),
            h('a.navbar-item', x => x.set({ href: '/login' }),
                Icon('person-12-regular'), ' Login',
            ),
            h('a.navbar-item', x => x.set({ href: '/join' }),
                Icon('sparkle-16-regular'), ' Join',
            ),
        ),
        h('.navbar-group'),
    );
}
