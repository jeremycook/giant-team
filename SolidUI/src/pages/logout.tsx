import { Show } from 'solid-js';
import { A, PageSettings } from '../partials/Nav';
import { logout, isAuthenticated } from '../utils/session';

export const pageSettings: PageSettings = {
  name: 'Logout',
  showInNav: () => isAuthenticated(),
}

export default function LogoutPage() {
  logout();

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>Logout</h1>

      <Show when={!isAuthenticated()} fallback={
        <p>
          One moment please, logging out…
        </p>
      }>
        <p>
          You have been logged out.
        </p>
        <ul>
          <li><A href='/'>Go home</A></li>
          <li><A href='/login'>Login</A></li>
        </ul>
      </Show>

    </section>
  );
}
