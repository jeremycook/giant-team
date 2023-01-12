import { createStore } from 'solid-js/store';
import { postLogout, postSession } from '../bindings/GiantTeam.Authentication.Api.Controllers';

const KEY = 'SESSION388';

/** Refresh every 30 minutes if still authenticated to keep the session alive. */
setInterval(() => user.isAuthenticated && user.refresh(), 30 * 60 * 1000);

export class User {
    public get isAuthenticated() {
        return _session.isAuthenticated;
    }

    public get username() {
        return _session.username;
    }

    public get userId() {
        return _session.userId;
    }

    /** Refresh the session from the server */
    public async refresh() {
        console.debug('Refreshing session.');

        var response = await postSession();

        if (response.ok) {
            this._setSession({
                isAuthenticated: true,
                userId: response.data.userId ?? undefined,
                username: response.data.username ?? undefined,
            });
        }
    }

    /** Logout from the server and clear the session */
    public async logout() {
        console.debug('Logging out.');

        await postLogout();

        this._setSession({ isAuthenticated: false });
    }

    private _setSession(session: Session) {
        sessionStorage.setItem(KEY, JSON.stringify(session));
        _setSession(session);
    }
}

export const user = new User();

export interface Session {
    isAuthenticated: boolean;
    userId?: string;
    username?: string;
}

const [_session, _setSession] = (() => {
    const json = sessionStorage.getItem(KEY);

    if (typeof json === 'string') {
        const session = JSON.parse(json);
        return createStore<Session>({
            isAuthenticated: session.isAuthenticated === true,
            userId: session.userId ?? undefined,
            username: session.username ?? undefined,
        });
    }
    else {
        return createStore<Session>({
            isAuthenticated: false,
        });
    }
})()
