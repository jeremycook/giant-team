import { A, PageSettings } from "../../partials/Nav";
import { isAuthenticated, session } from "../../utils/session";
import MyOrganizations from "../organizations/partials/MyOrganizations";

export const pageSettings: PageSettings = {
    name: 'My Profile',
    showInNav: () => isAuthenticated(),
}

export default function MyPage() {
    return (
        <section class='card md:w-md md:mx-auto'>
            <h1>My Profile</h1>
            <p>
                Welcome {session.username}!
            </p>

            <h2>My Organizations</h2>
            <div class='flex gap-4'>
                <MyOrganizations />
                <div class='card'>
                    <A href='/organizations/new-organization'>New Organization</A>
                </div>
            </div>
        </section>
    )
}