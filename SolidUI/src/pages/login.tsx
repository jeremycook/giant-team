import { createSignal, Show } from 'solid-js';
import { postLogin } from '../api/GiantTeam.Authentication.Api';
import { isAuthenticated, refreshSession, session } from '../utils/session';
import { InfoIcon, WarningIcon } from '../helpers/icons';
import { FieldStack, FieldSetOptions } from '../widgets/FieldStack';
import { createMutable } from 'solid-js/store';
import { A, getState, go, PageInfo } from '../partials/Nav';
import { isLocalUrl } from '../helpers/urlHelpers';

const dataOptions: FieldSetOptions = {
  username: { type: 'text', label: 'Username', required: true, autocomplete: 'username' },
  password: { type: 'password', label: 'Password', autocomplete: 'current-password' },
  remainLoggedIn: { type: 'boolean', label: 'Remember me' },
};

export const pageInfo: PageInfo = {
  name: 'Login',
  showInNav: () => !isAuthenticated(),
}

export default function LoginPage() {
  const state = getState<{ returnUrl?: string } | undefined>();

  const data = createMutable({
    username: '',
    password: '',
    remainLoggedIn: false,
  });

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal('');

  const returnUrl = () => {
    if (state?.returnUrl) {
      const url = new URL(state.returnUrl, location.href);
      if (isLocalUrl(url) && !url.pathname.endsWith('/login'))
        return url.toString();
    }

    // Fallback
    return '/profile';
  };

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;

    const output = await postLogin({
      username: form.username.value,
      password: form.password.value,
      remainLoggedIn: form.remainLoggedIn.checked,
    });

    okSetter(output.ok);

    if (output.ok) {
      messageSetter('Logging you in…');

      // Refresh the session
      await refreshSession()

      const url = returnUrl();
      console.debug(`Redirecting from ${window.location.href} to ${returnUrl}.`)
      go(url, { replace: true });
      return;
    } else {
      messageSetter(output.message);
    }
  };

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>Login</h1>

      <Show when={isAuthenticated()}>
        <p class='text-info' role='alert'>
          <InfoIcon class='animate-bounce-in' />{' '}
          FYI: You are currently logged in as <A href='/profile'>{session.username}</A>.
          <Show when={returnUrl()}>
            {' '}<A href={returnUrl()!} class='underline'>Click here to go back</A>.
          </Show>
        </p>
      </Show>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          <WarningIcon class='animate-bounce-in' />{' '}
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit} class='form-grid'>

        <FieldStack data={data} options={dataOptions} />

        <div />
        <div>
          <button type='submit' class='button paint-primary'>
            Login
          </button>
          <A href='/join' class='p-button'>Join</A>
        </div>

      </form>

    </section>
  );
}
