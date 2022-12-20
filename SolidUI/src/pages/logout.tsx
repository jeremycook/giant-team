import { Show } from 'solid-js';
import { logout, isAuthenticated } from '../utils/session';
import { A } from '@solidjs/router';
import { setTitle, title } from '../utils/page';

export default function LogoutPage() {
  setTitle('Logout');

  logout();

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>{title()}</h1>

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
