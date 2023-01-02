import { PageInfo } from "../partials/Nav";
import { isAuthenticated, session } from "../utils/session";

export const pageInfo: PageInfo = {
    name: 'Profile',
    showInNav: () => isAuthenticated(),
}

export default function ProfilePage() {
    return (
        <section class='card md:w-md md:mx-auto'>
            <h1>My Profile</h1>
            <p>
                Welcome {session.username}!
            </p>
        </section>
    )
}