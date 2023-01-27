import { postSession, postLogout, SessionStatus } from "../../bindings/Authentication.Api.Controllers";
import { compare } from "../../helpers/compare";
import { parseJson } from "../../helpers/objectHelpers";

const SESSION_STORAGE_KEY = 'user_state';

type UserState =
    | { isAuthenticated: false; }
    | { isAuthenticated: true; userId: string; username: string; }

export enum UserEvent {
    loggedin = 'user_loggedin',
    loggedout = 'user_loggedout',
}

export class User {
    private _state: UserState = {
        isAuthenticated: false,
    }

    get isAuthenticated() {
        return this._state.isAuthenticated;
    }

    get username() {
        return this._state.isAuthenticated
            ? this._state.username
            : undefined;
    }

    get userId() {
        return this._state.isAuthenticated
            ? this._state.userId
            : undefined;
    }

    constructor() {
        this._loadState();
    }

    addEventListener(type: UserEvent, listener: EventListenerOrEventListenerObject) {
        document.addEventListener(type, listener);
    }

    removeEventListener(type: UserEvent, listener: EventListenerOrEventListenerObject) {
        document.removeEventListener(type, listener);
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

            if (!compare.same(state, this._state)) {
                this._state = state;
                this._saveState();
                this._dispatchEvent();
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

            this._state = { isAuthenticated: false };
            this._saveState();
            this._dispatchEvent();
        }
    }

    private _dispatchEvent() {
        const type = this._state.isAuthenticated
            ? UserEvent.loggedin
            : UserEvent.loggedout;
        document.dispatchEvent(new CustomEvent(type));
    }

    private _loadState() {
        const json = sessionStorage.getItem(SESSION_STORAGE_KEY);
        if (json) {
            this._state = parseJson(json);
        }
    }

    private _saveState() {
        sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(this._state));
    }
}

export const user = new User();

/** Refresh every 30 minutes if authenticated to keep the session alive. */
setInterval(() => user.isAuthenticated && user.refresh(), 30 * 60 * 1000);
