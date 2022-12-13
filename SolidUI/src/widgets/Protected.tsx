import { Navigate, Outlet, useLocation } from "@solidjs/router";
import { Show } from "solid-js";
import { isAuthenticated } from "../session";

export const Protected = () => {
    const location = useLocation();

    return (
        <Show when={isAuthenticated()}
            fallback={
                <Navigate href='/login' state={{ returnUrl: location.pathname + location.search + location.hash }} />
            }>
            <Outlet />
        </Show>
    );
}

export default Protected;
