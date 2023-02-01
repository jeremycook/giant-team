import { h } from '../../helpers/h';
import { user } from '../login/user';
import MainLayout from '../_ui/MainLayout';
import MyOrganizations from './_ui/MyOrganizations';


export default function MyPage() {
    return MainLayout(
        h('h1', 'My Profile'),
        h('p', user.username!),

        h('h2', 'My Organizations'),
        h('.flex.flex-wrap.gap-100',
            MyOrganizations(),
            h('.card-new',
                h('a', x => x.set({ href: '/organizations/new-organization' }), 'New Organization'),
            )
        )
    )
}