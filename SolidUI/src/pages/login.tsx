import { A, useLocation, useNavigate } from '@solidjs/router';
import { createSignal, Show } from 'solid-js';
import { postLogin, SessionStatus } from '../api/GiantTeam.Authentication.Api';
import { refreshSession, session } from '../session';
import { titleSetter } from '../title';
import { createId } from '../utils/htmlHelpers';
import { InfoIcon, WarningIcon } from '../utils/iconHelpers';

export default function Login() {
  titleSetter("Login");

  const location = useLocation();
  const navigate = useNavigate();

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal("");

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;

    const output = await postLogin({
      username: form.username.value,
      password: form.password.value,
      remainLoggedIn: form.remainLoggedIn.checked,
    });

    okSetter(output.ok);
    console.log(output);

    if (output.ok) {
      messageSetter("Logging you in…");

      // Refresh the session
      await refreshSession()

      if (location.state && typeof (location.state as any).returnUrl === "string") {
        const returnUrl = (location.state as any).returnUrl as string;
        if (returnUrl.startsWith('/') && !returnUrl.endsWith("/login")) {
          // Redirect to page that triggered login flow
          console.log(`Redirecting from ${window.location.href} to ${returnUrl}.`)
          navigate(returnUrl);
        }
      }

      // Fallback to home page
      console.log(`Redirecting from ${window.location.href} to /.`)
      navigate('/');
    } else {
      messageSetter(output.message);
    }
  };

  return (
    <section class="card md:w-md">

      <h1>Login</h1>

      <Show when={session().status === SessionStatus.Authenticated}>
        <p class="text-info" role="alert">
          <InfoIcon class="animate-bounce-in" />{' '}
          FYI: You are currently logged in as <A href="/profile">{session().username}</A>,
          but you can login as someone else if you need to.
        </p>
      </Show>

      <Show when={message()}>
        <p class={(ok() ? "text-ok" : "text-error")} role="alert">
          <WarningIcon class="animate-bounce-in" />{' '}
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit} class="form-grid">

        <label for={createId("username")}>
          Username
        </label>
        <input
          id={createId("username")}
          name="username"
          required
          autofocus
        />

        <label for={createId("password")}>
          Password
        </label>
        <input
          id={createId("password")}
          name="password"
          type="password"
          required
        />

        <div />
        <label><input
          name="remainLoggedIn"
          type="checkbox"
        /> Keep me logged in</label>

        <div />
        <div>
          <button type="submit" class="button">
            Login
          </button>
          <A href="/join" class="p-button">Join</A>
        </div>

      </form>

    </section>
  );
}
