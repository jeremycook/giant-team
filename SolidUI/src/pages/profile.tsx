import { session } from "../utils/session";
import { setTitle, title } from "../utils/title";

export default function ProfilePage() {
    setTitle('Welcome ' + session.username + '!');

    return (
        <section class='card md:w-md md:mx-auto'>
            <h1>{title()}</h1>
            <p>
                TOOD: More coming soon…
            </p>
        </section>
    )
}