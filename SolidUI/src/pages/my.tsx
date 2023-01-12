import { Anchor } from "../partials/Anchor";
import { MainLayout } from "../partials/MainLayout";
import { user } from "../utils/session";
import MyOrganizations from "./organizations/partials/MyOrganizations";

export default function MyPage() {
    return (
        <MainLayout>
            <h1>My Profile</h1>
            <p>
                Welcome {user.username}!
            </p>

            <h2>My Organizations</h2>
            <div class='flex gap-4'>
                <MyOrganizations />
                <div class='card-new'>
                    <Anchor href='/organizations/new-organization'>New Organization</Anchor>
                </div>
            </div>
        </MainLayout>
    )
}