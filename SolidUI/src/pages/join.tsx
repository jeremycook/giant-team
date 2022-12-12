import { createEffect, createSignal, Show } from 'solid-js';
import { postRegister } from '../api/GiantTeam.Authentication.Api';
import { titleSetter } from '../title';
import { createId } from '../utils/htmlHelpers';
import { createUrl } from '../utils/urlHelpers';

export default function Register() {
  titleSetter("Join");

  // Input
  const [name, nameSetter] = createSignal("");
  const [email, emailSetter] = createSignal("");
  const [username, usernameSetter] = createSignal("");
  const [password, passwordSetter] = createSignal("");
  const [passwordConfirmation, passwordConfirmationSetter] = createSignal("");

  // Output
  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal("");

  createEffect(() => {
    if (!username() && name())
      usernameSetter(name().toLowerCase().replace(/[^a-z0-9]+/, "-"))
  });

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    // Client-side validation
    if (password() !== passwordConfirmation()) {
      okSetter(false);
      messageSetter("The password and password confirmation fields must match.");
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
      messageSetter("Success! Redirecting to the login page…");
      // TODO: Redirect to page that triggered login flow
      location.replace(createUrl("/login", { username: username() }));
    } else {
      messageSetter(output.message);
    }
  };

  return (
    <section class="card md:w-md">

      <h1>Register</h1>

      <Show when={message()}>
        <p class={(ok() ? "text-ok" : "text-error")} role="alert">
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit}>

        <div>
          <label for={createId("name")}>
            Name
          </label>
          <input
            id={createId("name")}
            value={name()}
            onChange={e => nameSetter(e.currentTarget.value)}
            required
            autofocus
          />
        </div>

        <div>
          <label for={createId("email")}>
            Email
          </label>
          <input
            id={createId("email")}
            value={email()}
            onChange={e => emailSetter(e.currentTarget.value)}
            required
            type="email"
          />
        </div>

        <div>
          <label for={createId("username")}>
            Username
          </label>
          <input
            id={createId("username")}
            value={username()}
            onChange={e => usernameSetter(e.currentTarget.value)}
            required
          />
        </div>

        <div>
          <label for={createId("password")}>
            Password
          </label>
          <input
            id={createId("password")}
            value={password()}
            onChange={e => passwordSetter(e.currentTarget.value)}
            required
            type="password"
          />
        </div>

        <div>
          <label for={createId("passwordConfirmation")}>
            Password Confirmation
          </label>
          <input
            id={createId("passwordConfirmation")}
            value={passwordConfirmation()}
            onChange={e => passwordConfirmationSetter(e.currentTarget.value)}
            required
            type="password"
          />
        </div>

        <button type="submit">
          Register
        </button>

      </form>

    </section>
  );
}
