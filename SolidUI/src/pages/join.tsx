import { useNavigate } from '@solidjs/router';
import { createEffect, createSignal, Show } from 'solid-js';
import { postRegister } from '../api/GiantTeam.Authentication.Api';
import { title, setTitle } from '../title';
import { createId } from '../helpers/htmlHelpers';
import { createUrl } from '../helpers/urlHelpers';

export default function JoinPage() {
  setTitle('Join');

  const navigate = useNavigate();

  // Input
  const [name, nameSetter] = createSignal('');
  const [email, emailSetter] = createSignal('');
  const [username, usernameSetter] = createSignal('');
  const [password, passwordSetter] = createSignal('');
  const [passwordConfirmation, passwordConfirmationSetter] = createSignal('');

  // Output
  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal('');

  createEffect(() => {
    if (!username() && name())
      usernameSetter(name().toLowerCase().replace(/[^a-z0-9]+/, '-'))
  });

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    // Client-side validation
    if (password() !== passwordConfirmation()) {
      okSetter(false);
      messageSetter('The password and password confirmation fields must match.');
      return;
    }

    const output = await postRegister({
      name: name(),
      email: email(),
      username: username(),
      password: password(),
    });

    okSetter(output.ok);

    if (output.ok) {
      messageSetter('Success! Redirecting to the login page…');
      // TODO: Redirect to page that triggered login flow
      navigate(createUrl('/login', { username: username() }));
    } else {
      messageSetter(output.message);
    }
  };

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>{title()}</h1>

      <p>
        Register a new user account.
      </p>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit} class='form-grid'>

        <label for={createId('name')}>
          Name
        </label>
        <div>
          <input
            id={createId('name')}
            value={name()}
            onChange={e => nameSetter(e.currentTarget.value)}
            required
            autofocus
          />
        </div>

        <label for={createId('email')}>
          Email
        </label>
        <div>
          <input
            id={createId('email')}
            value={email()}
            onChange={e => emailSetter(e.currentTarget.value)}
            required
            type='email'
          />
        </div>

        <label for={createId('username')}>
          Username
        </label>
        <div>
          <input
            id={createId('username')}
            value={username()}
            onChange={e => usernameSetter(e.currentTarget.value)}
            required
            autocomplete='username'
          />
        </div>

        <label for={createId('password')}>
          Password
        </label>
        <div>
          <input
            id={createId('password')}
            value={password()}
            onChange={e => passwordSetter(e.currentTarget.value)}
            required
            type='password'
            autocomplete='new-password'
          />
        </div>

        <label for={createId('passwordConfirmation')}>
          Password Confirmation
        </label>
        <div>
          <input
            id={createId('passwordConfirmation')}
            value={passwordConfirmation()}
            onChange={e => passwordConfirmationSetter(e.currentTarget.value)}
            required
            type='password'
            autocomplete='new-password'
          />
        </div>

        <div />
        <div>
          <button class='button'>
            Register
          </button>
        </div>

      </form>

    </section>
  );
}
