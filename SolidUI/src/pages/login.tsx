import { createSignal, Show } from 'solid-js';
import { postLogin } from '../api/GiantTeam.Authentication.Api';
import { createId } from '../utils/htmlHelpers';

export default function Login() {

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

    console.log(output);

    okSetter(output.ok);

    if (output.ok) {
      messageSetter("Logging you in…");
      if (typeof history.state === "object" && typeof history.state.returnUrl === "string") {
        // Redirect to page that triggered login flow
        location.replace(history.state.returnUrl);
      }
      else {
        location.replace("/");
      }
    } else {
      messageSetter(output.message);
    }
  };

  return (
    <section>

      <h1>Login</h1>

      <Show when={message()}>
        <p class={(ok() ? "text-green" : "text-red")}>
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit}>

        <div>
          <label for={createId("username")}>
            Username
          </label>
          <input
            id={createId("username")}
            name="username"
            required
            autofocus
          />
        </div>

        <div>
          <label for={createId("password")}>
            Password
          </label>
          <input
            id={createId("password")}
            name="password"
            type="password"
            required
          />
        </div>

        <div>
          <label><input
            name="remainLoggedIn"
            type="checkbox"
          /> Keep me logged in</label>
        </div>

        <button type="submit">
          Login
        </button>

      </form>

    </section>
  );
}
