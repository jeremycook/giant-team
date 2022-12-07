import { createSignal } from "solid-js";
import { postLogout, postSession, SessionOutput, SessionStatus } from "./api/GiantTeam.Authentication.Api";

export const [session, sessionSetter] = createSignal(<SessionOutput>{
    status: SessionStatus.Anonymous,
    name: null,
});

/** Logout from the server and refresh the session */
export const disconnectSession = async () => {
    await postLogout();
    await refreshSession();
}

/** Refresh the session from the server */
export const refreshSession = async () => {
    var response = await postSession();
    sessionSetter(response.data!);
}

refreshSession();
