import { createSignal, Show } from 'solid-js';
import { isAuthenticated, refreshSession, session } from '../utils/session';
import { InfoIcon, WarningIcon } from '../helpers/icons';
import { FieldStack, FieldSetOptions } from '../widgets/FieldStack';
import { createMutable } from 'solid-js/store';
import { Anchor } from '../partials/Anchor';
import { isLocalUrl } from '../helpers/urlHelpers';
import { postLogin } from '../bindings/GiantTeam.Authentication.Api.Controllers';
import { useLocation, useNavigate } from '@solidjs/router';

const dataOptions: FieldSetOptions = {
  username: { type: 'text', label: 'Username', required: true, autocomplete: 'username' },
  password: { type: 'password', label: 'Password', autocomplete: 'current-password' },
  remainLoggedIn: { type: 'boolean', label: 'Remember me' },
};

export default function LoginPage() {
  const here = useLocation<{ username: string, returnUrl: string }>();
  const navigate = useNavigate();

  const data = createMutable({
    username: here.state?.username ?? '',
    password: '',
    remainLoggedIn: false,
  });

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal('');

  const returnUrl = () => {
    if (here.state?.returnUrl) {
      const url = new URL(here.state?.returnUrl, location.href);
      if (isLocalUrl(url) && !url.pathname.endsWith('/login'))
        return url.toString();
    }

    // Fallback
    return '/my';
  };

  const onSubmitForm = async (e: SubmitEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;

    const output = await postLogin({
      username: form.username.value,
      password: form.password.value,
      elevated: true, // TODO: toggle
      remainLoggedIn: form.remainLoggedIn.checked,
    });

    okSetter(output.ok);

    if (output.ok) {
      messageSetter('Logging you in…');

      // Refresh the session
      await refreshSession()

      const url = returnUrl();
      console.debug(`Redirecting from ${location.href} to ${url}.`)
      navigate(url);
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
          FYI: You are currently logged in as <Anchor href='/profile'>{session.username}</Anchor>.
          <Show when={returnUrl()}>
            {' '}<Anchor href={returnUrl()!} class='underline'>Click here to go back</Anchor>.
          </Show>
        </p>
      </Show>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          <WarningIcon class='animate-bounce-in' />{' '}
          {message()}
        </p>
      </Show>

      <form onSubmit={onSubmitForm} class='form-grid'>

        <FieldStack data={data} options={dataOptions} />

        <div />
        <div>
          <button type='submit' class='button paint-primary'>
            Login
          </button>
          <Anchor href='/join' class='p-button'>Join</Anchor>
        </div>

      </form>

    </section>
  );
}
