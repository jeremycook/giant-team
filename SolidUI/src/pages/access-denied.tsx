import { CardLayout } from "../partials/CardLayout";

// TODO: Customize the error message with username and the URL or more info about the resource they were trying to access.
export default function AccessDeniedPage() {
    return (
        <CardLayout>
            <h1>Access Denied</h1>
            <p class='text-error' role='alert'>
                Your user account does not have permission access the requested resource.
            </p>
            <p>
                Would you like to <a href='/login'>login as a different user</a>?
            </p>
        </CardLayout>
    );
}
