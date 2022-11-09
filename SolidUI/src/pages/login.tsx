import { createSignal, Show } from 'solid-js';
import { postLogin, LoginStatus } from '../api/GiantTeam.Authentication.Api';
import { createId } from '../utils/elementHelpers';

export default function Login() {
  // Input
  const [username, usernameSetter] = createSignal("");
  const [password, passwordSetter] = createSignal("");
  const [remainLoggedIn, remainLoggedInSetter] = createSignal(false);

  // Output
  const [message, messageSetter] = createSignal("");

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const output = await postLogin({
      username: username(),
      password: password(),
      remainLoggedIn: remainLoggedIn(),
    });

    console.log(output);

    switch (output.status) {
      case LoginStatus.Authenticated:
        messageSetter("Logging you in…");
        location.replace("/");
        break;
      case LoginStatus.InvalidInput:
        messageSetter(output.message);
        break;
      default:
        throw Error(`Unsupported LoginStatus ${output.status}.`);
    }
  };

  return (
    <section>

      <h1>Login</h1>

      <Show when={message()}>
        <div class="text-red">
          {message()}
        </div>
      </Show>

      <form onSubmit={formSubmit}>

        <div>
          <label for={createId("Username")}>
            Username
          </label>
          <input
            id={createId("Username")}
            value={username()}
            onChange={e => usernameSetter(e.currentTarget.value)}
            required
            autofocus
          />
        </div>

        <div>
          <label for={createId("Password")}>
            Password
          </label>
          <input
            id={createId("Password")}
            type="password"
            value={password()}
            onChange={e => passwordSetter(e.currentTarget.value)}
            required
          />
        </div>

        <div>
          <label><input
            type="checkbox"
            checked={remainLoggedIn()}
            onChange={e => remainLoggedInSetter(e.currentTarget.checked)}
          /> Keep me logged in</label>
        </div>

        <button type="submit">
          Login
        </button>

      </form>

    </section>
  );
}
