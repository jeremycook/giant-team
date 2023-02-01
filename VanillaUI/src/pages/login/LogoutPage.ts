import { f, h } from '../../helpers/h';
import CardLayout from "../_ui/CardLayout";
import { user } from "./user";

export default function LogoutPage() {
    // user.logout()

    setTimeout(() => user.logout(), 1000);

    return CardLayout(
        h('h1', 'Logout'),
        user.pipe.map(u => u.isAuthenticated
            ? h('p', 'One moment please, logging out…')
            : f(
                h('p', 'You have been logged out.'),
                h('ul',
                    h('li', h('a', x => x.set({ 'href': '/' }), 'Go home')),
                    h('li', h('a', x => x.set({ 'href': '/login' }), 'Login')),
                ),
            )
        ),
    )
}
