import { PageInfo } from "../partials/Nav";

export const pageInfo: PageInfo = {
    name: 'Home',
}

export default function HomePage() {
    return (
        <section class='card md:w-md md:mx-auto'>
            <h1>Welcome!</h1>
            <p>
                We'll put something useful here soon!
            </p>
        </section>
    );
}
