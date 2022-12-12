import { useLocation, useNavigate } from '@solidjs/router';
import { createSignal } from 'solid-js';
import { postLogout, postSession, SessionOutput, SessionStatus } from './api/GiantTeam.Authentication.Api';

export interface Session extends SessionOutput {
    status: SessionStatus | -1;
}

export const [session, sessionSetter] = createSignal(<Session>{
    status: -1
});

/** Logout from the server and refresh the session */
export const disconnectSession = async () => {
    await postLogout();
    await refreshSession();
}

/** Refresh the session from the server */
export const refreshSession = async () => {
    var response = await postSession();

    if (response.data) {
        sessionSetter(response.data);
        console.debug('Refreshed session', response.data);
    }
}

/** Force anonymous users to login. */
export const authorize = async () => {
    if (session().status === -1) {
        await refreshSession();
    }

    // Redirect for now, popup login in the future
    if (session().status !== SessionStatus.Authenticated) {
        const location = useLocation();
        const navigate = useNavigate();

        const url = location.pathname + location.search + location.hash;
        
        console.log(`Redirecting from ${url} to /login.`)
        navigate('/login', { replace: false, state: { returnUrl: url } });
    }
}

setTimeout(() => refreshSession(), 1);
