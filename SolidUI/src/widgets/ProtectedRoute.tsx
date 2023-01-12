import { Outlet } from "@solidjs/router";
import { Show } from "solid-js";
import { Login } from "../pages/login";
import { CardLayout } from "../partials/CardLayout";
import { user } from "../utils/session";

export const ProtectedRoute = () => {
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

export default { ProtectedRoute };
