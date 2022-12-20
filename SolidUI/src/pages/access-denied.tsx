import { createSignal, Show } from 'solid-js';
import { setTitle, title } from '../utils/title';

export default function AccessDeniedPage() {
  setTitle('Access Denied');

  const [ok] = createSignal(false);
  const [message] = createSignal('Your user account does not have permission access the requested resource.');

  // TODO: Customize the error message with username and the URL or more info about the resource they were trying to access.

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>{title()}</h1>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          {message()}
        </p>
      </Show>

      <p>
        Would you like to <a href='/login'>login as a different user</a>?
      </p>

    </section>
  );
}
