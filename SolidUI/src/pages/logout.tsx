import { Show } from 'solid-js';
import { Anchor } from '../partials/Anchor';
import { logout, isAuthenticated } from '../utils/session';

export const pageSettings = {
  name: 'Logout',
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
          <li><Anchor href='/'>Go home</Anchor></li>
          <li><Anchor href='/login'>Login</Anchor></li>
        </ul>
      </Show>

    </section>
  );
}
