import { createEffect, createSignal, createUniqueId, Show } from 'solid-js';
import { postRegister, RegisterStatus } from '../api/GiantTeam.Authentication.Api';

const getId = (suffix: string) =>
  createUniqueId() + "_" + suffix;

export default function Register() {
  // Input
  const [name, nameSetter] = createSignal("");
  const [email, emailSetter] = createSignal("");
  const [username, usernameSetter] = createSignal("");
  const [password, passwordSetter] = createSignal("");
  const [passwordConfirmation, passwordConfirmationSetter] = createSignal("");

  // Output
  const [message, messageSetter] = createSignal("");

  createEffect(() => {
    if (!username() && name())
      usernameSetter(name().toLowerCase().replace(/[^a-z0-9]+/, "-"))
  });

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const output = await postRegister({
      name: name(),
      email: email(),
      username: username(),
      password: password(),
      passwordConfirmation: passwordConfirmation(),
    });

    console.log(output);

    switch (output.status) {
      case RegisterStatus.Success:
        messageSetter("Success! Redirecting to the login page…");
        location.replace("/login");
        break;
      case RegisterStatus.Problem:
        messageSetter(output.message);
        break;
      default:
        throw Error(`Unsupported RegisterStatus ${output.status}.`);
    }
  };

  return (
    <section>

      <h1>Register</h1>

      <Show when={message()}>
        <div class="text-red">
          {message()}
        </div>
      </Show>

      <form onSubmit={formSubmit}>

        <div>
          <label for={getId("Name")}>
            Name
          </label>
          <input
            id={getId("Name")}
            value={name()}
            onChange={e => nameSetter(e.currentTarget.value)}
            required
            autofocus
          />
        </div>

        <div>
          <label for={getId("Email")}>
            Email
          </label>
          <input
            id={getId("Email")}
            value={email()}
            onChange={e => emailSetter(e.currentTarget.value)}
            required
            type="email"
          />
        </div>

        <div>
          <label for={getId("Username")}>
            Username
          </label>
          <input
            id={getId("Username")}
            value={username()}
            onChange={e => usernameSetter(e.currentTarget.value)}
            required
          />
        </div>

        <div>
          <label for={getId("Password")}>
            Password
          </label>
          <input
            id={getId("Password")}
            value={password()}
            onChange={e => passwordSetter(e.currentTarget.value)}
            required
            type="password"
          />
        </div>

        <div>
          <label for={getId("PasswordConfirmation")}>
            Password Confirmation
          </label>
          <input
            id={getId("PasswordConfirmation")}
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
