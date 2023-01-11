import { Navigate, Outlet, useLocation } from "@solidjs/router";
import { Show } from "solid-js";
import { user } from "../utils/session";

export const ProtectedRoute = () => {
    const location = useLocation();

    return (
        <Show when={user.isAuthenticated}
            fallback={
                <Navigate href='/login' state={{ returnUrl: location.pathname + location.search + location.hash }} />
            }>
            <Outlet />
        </Show>
    );
}

export default { ProtectedRoute };
