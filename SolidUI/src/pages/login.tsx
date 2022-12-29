import { A, useLocation, useNavigate } from '@solidjs/router';
import { createSignal, onCleanup, Show } from 'solid-js';
import { postLogin, SessionStatus } from '../api/GiantTeam.Authentication.Api';
import { isAuthenticated, refreshSession, session } from '../utils/session';
import { setTitle } from '../utils/page';
import { InfoIcon, WarningIcon } from '../helpers/icons';
import { FieldStack, FieldSetOptions } from '../widgets/FieldStack';
import { createMutable } from 'solid-js/store';

const dataOptions: FieldSetOptions = {
  username: { type: 'text', label: 'Username', required: true, autocomplete: 'username' },
  password: { type: 'password', label: 'Password', autocomplete: 'current-password' },
  remainLoggedIn: { type: 'boolean', label: 'Keep me logged in' },
};

export default function LoginPage() {
  setTitle('Login');

  const location = useLocation<{ returnUrl: string | undefined }>();
  const navigate = useNavigate();

  const data = createMutable({
    username: '',
    password: '',
    remainLoggedIn: false,
  });

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal('');

  const returnUrl = () =>
    location.state?.returnUrl?.startsWith('/') && !location.state?.returnUrl?.endsWith('/login') ?
      location.state?.returnUrl :
      null;

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

      if (typeof returnUrl() === 'string') {
        // Redirect to page that triggered login flow
        console.debug(`Redirecting from ${window.location.href} to ${returnUrl}.`)
        navigate(returnUrl()!, { replace: true });
      }
      else {
        // Fallback to home page
        console.debug(`Redirecting from ${window.location.href} to /.`)
        navigate('/');
      }
      return;
    } else {
      messageSetter(output.message);
    }
  };

  if (!isAuthenticated()) {
    // TODO: Only do this in development env
    const refresher = setInterval(() => isAuthenticated() ? clearInterval(refresher) : refreshSession(), 5 * 1000);
    onCleanup(() => clearTimeout(refresher));
  }

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>Login</h1>

      <Show when={session.status === SessionStatus.Authenticated}>
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
