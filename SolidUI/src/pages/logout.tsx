import { Show } from 'solid-js';
import { SessionStatus } from '../api/GiantTeam.Authentication.Api';
import { disconnectSession, session } from '../session';
import { Link } from 'solid-app-router';

export default function Login() {

  disconnectSession();

  return (
    <section>

      <h1>Logout</h1>

      <p>
        <Show when={session().status === SessionStatus.Anonymous} fallback={
          <>One moment please, logging out…</>
        }>
          You have been logged out.
          <ul>
            <li><Link href="/">Go home</Link></li>
            <li><Link href="/login">Log back in</Link></li>
          </ul>
        </Show>
      </p>

    </section >
  );
}
