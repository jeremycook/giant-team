import { useLocation, useNavigate } from '@solidjs/router';
import { createStore } from 'solid-js/store';
import { postLogout, postSession, SessionOutput, SessionStatus } from '../api/GiantTeam.Authentication.Api';

/** Keep the session alive */
setInterval(() => isAuthenticated() && refreshSession(), 30 * 60 * 1000);

export interface Session extends SessionOutput {
  status: SessionStatus | -1;
}

export const [session, setSession] = createStore<Session>({
  status: -1
});

export const isAuthenticated = () => session.status === SessionStatus.Authenticated;

/** Logout from the server and refresh the session */
export const logout = async () => {
  console.debug('Logging out.');

  setSession({ status: -1 });

  await postLogout();
}

/** Refresh the session from the server */
export const refreshSession = async () => {
  console.debug('Refreshing session.');

  var response = await postSession();

  if (response.ok) {
    setSession(response.data);
  }
}

/** Present the login page to anonymous users. */
export const authorize = () => {
  // Redirect for now, popup login in the future
  if (!isAuthenticated()) {
    const location = useLocation();
    const navigate = useNavigate();

    const url = location.pathname + location.search + location.hash;

    console.debug(`Redirecting from ${url} to /login.`);
    navigate('/login', { replace: false, state: { returnUrl: url } });
  }
}
