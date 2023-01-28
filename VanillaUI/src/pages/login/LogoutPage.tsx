import On from "../../helpers/jsx/On";
import CardLayout from "../_ui/CardLayout";
import { user, UserEvent } from "./user";

export default function LogoutPage() {
    // user.logout()

    setTimeout(() => user.logout(), 2000);

    return <CardLayout>

        <h1>Logout</h1>

        <On events={[UserEvent.loggedin, UserEvent.loggedout]}>{() => {
            if (user.isAuthenticated) {
                return <>
                    <p>
                        One moment please, logging out…
                    </p>
                </>
            }
            else {
                return <>
                    <p>
                        You have been logged out.
                    </p>
                    <ul>
                        <li><a href='/'>Go home</a></li>
                        <li><a href='/login'>Login</a></li>
                    </ul>
                </>
            }
        }}</On>
    </CardLayout>;
}
