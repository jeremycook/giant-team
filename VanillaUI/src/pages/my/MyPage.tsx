import { user } from '../login/user';
import MainLayout from '../_ui/MainLayout';
import MyOrganizations from './_ui/MyOrganizations';


export default function MyPage() {
    return (
        <MainLayout>
            <h1>My Profile</h1>
            <p>
                Welcome {user.username}!
            </p>

            <h2>My Organizations</h2>
            <div class='flex flex-wrap gap-100'>
                <MyOrganizations />
                <div class='card-new'>
                    <a href='/organizations/new-organization'>New Organization</a>
                </div>
            </div>
        </MainLayout>
    )
}