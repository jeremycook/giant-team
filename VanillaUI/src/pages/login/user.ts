import { postSession, postLogout, SessionStatus } from "../../bindings/Authentication.Api.Controllers";
import { compare } from "../../helpers/compare";
import { parseJson } from "../../helpers/objectHelpers";
import { Pipe, State } from '../../helpers/Pipe';

const SESSION_STORAGE_KEY = 'user_state';

type UserState =
    | { isAuthenticated: false; }
    | { isAuthenticated: true; userId: string; username: string; }

export class User {
    private _state = new State<UserState>({
        isAuthenticated: false,
    })

    get pipe(): Pipe<UserState> {
        return this._state;
    }

    get isAuthenticated() {
        return this._state.value.isAuthenticated;
    }

    get username() {
        return this._state.value.isAuthenticated
            ? this._state.value.username
            : undefined;
    }

    get userId() {
        return this._state.value.isAuthenticated
            ? this._state.value.userId
            : undefined;
    }

    constructor() {
        this._loadState();
    }

    /** Refresh the session from the server */
    async refresh() {
        var response = await postSession();

        if (response.ok) {
            const state: UserState = response.data.status === SessionStatus.Authenticated
                ? {
                    isAuthenticated: true,
                    userId: response.data.userId!,
                    username: response.data.username!,
                }
                : {
                    isAuthenticated: false,
                };

            if (!compare.same(state, this._state.value)) {
                this._state.value = state;
                this._saveState();
            }
        }
        else {
            // Ignore errors like this
        }
    }

    /** Logout from the server and clear the session */
    async logout() {
        if (this.isAuthenticated) {
            console.debug('Logging out.');

            await postLogout();

            this._state.value = { isAuthenticated: false };
            this._saveState();
        }
    }

    private _loadState() {
        const json = sessionStorage.getItem(SESSION_STORAGE_KEY);
        if (json) {
            this._state.value = parseJson(json);
        }
    }

    private _saveState() {
        sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(this._state.value));
    }
}

export const user = new User();

/** Refresh every 30 minutes if authenticated to keep the session alive. */
setInterval(() => user.isAuthenticated && user.refresh(), 30 * 60 * 1000);
