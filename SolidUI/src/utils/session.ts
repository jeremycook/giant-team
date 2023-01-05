import { createStore } from 'solid-js/store';
import { postLogout, postSession } from '../bindings/GiantTeam.Authentication.Api.Controllers';
import { go } from '../partials/Nav';

const KEY = 'SESSION388';

/** Keep the session alive */
setInterval(() => isAuthenticated() && refreshSession(), 30 * 60 * 1000);

export interface Session {
  userId: string | null;
  username: string | null;
}

const [_session, _setSession] = (() => {
  const json = sessionStorage.getItem(KEY);

  console.log(KEY, json);

  if (typeof json === 'string')
    return createStore<Session>(JSON.parse(json));
  else {
    return createStore<Session>({
      userId: null,
      username: null,
    });
  }
})()

export const session = _session;

export const setSession = (session: Session) => {
  sessionStorage.setItem(KEY, JSON.stringify(session));
  _setSession(session);
}

export const isAuthenticated = () => session.userId ? true : false;

/** Logout from the server and refresh the session */
export const logout = async () => {
  console.debug('Logging out.');

  await postLogout();

  setSession({
    userId: null,
    username: null
  });
}

/** Refresh the session from the server */
export const refreshSession = async () => {
  console.debug('Refreshing session.');

  var response = await postSession();

  if (response.ok) {
    setSession(response.data as Session);
  }
}

/** Present the login page to anonymous users. */
export const authorize = () => {
  // Redirect for now, popup login in the future
  if (!isAuthenticated()) {
    const url = location.pathname + location.search + location.hash;

    console.debug(`Redirecting from ${url} to /login.`);

    go('/login', { returnUrl: url });
  }
}
