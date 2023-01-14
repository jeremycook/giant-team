import { Outlet, Route } from "@solidjs/router";
import { JSX, Show } from "solid-js";
import { Login } from "../pages/login";
import { CardLayout } from "../partials/CardLayout";
import { user } from "../utils/session";

export const ProtectedRouteComponent = () => {
    return <Show when={user.isAuthenticated}
        fallback={<>
            <CardLayout>
                <h1>
                    Login
                </h1>
                <Login username={user.username} returnUrl={false} />
            </CardLayout>
        </>}>
        <Outlet />
    </Show>
}

export const ProtectedRoute = (props: { children?: JSX.Element }) => {
    return <Route path='' component={ProtectedRouteComponent}>
        {props.children}
    </Route>
}

export default { ProtectedRouteComponent };
