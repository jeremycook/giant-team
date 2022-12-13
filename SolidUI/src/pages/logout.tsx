import { Show } from 'solid-js';
import { logout, isAuthenticated } from '../session';
import { A } from '@solidjs/router';
import { titleSetter } from '../title';

export default function LogoutPage() {
  titleSetter('Logout');

  logout();

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>Logout</h1>

      <p>
        <Show when={!isAuthenticated()} fallback={
          <>One moment please, logging out…</>
        }>
          You have been logged out.
          <ul>
            <li><A href='/'>Go home</A></li>
            <li><A href='/login'>Log back in</A></li>
          </ul>
        </Show>
      </p>

    </section >
  );
}
