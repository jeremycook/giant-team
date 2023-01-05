import { createEffect } from 'solid-js';
import { createMutable } from 'solid-js/store';
import { A, go, here, PageSettings } from '../partials/Nav';
import { toast } from '../partials/Alerts';
import { isAuthenticated } from '../utils/session';
import { FieldSetOptions, FieldStack } from '../widgets/FieldStack';
import { postRegister } from '../bindings/GiantTeam.Authentication.Api.Controllers';

export const pageSettings: PageSettings = {
  name: 'Join',
  showInNav: () => !isAuthenticated(),
}

export default function JoinPage() {

  const data = createMutable({
    name: '',
    username: '',
    taintedUsername: false,
    password: '',
    passwordConfirmation: '',
    email: '',
  });

  const dataOptions: FieldSetOptions = {
    name: { type: 'text', label: 'Name', required: true },
    username: { type: 'text', label: 'Username', required: true, autocomplete: 'new-username', onfocus: () => data.taintedUsername = true },
    password: { type: 'password', label: 'Password', autocomplete: 'new-password' },
    passwordConfirmation: { type: 'password', label: 'Password Confirmation', autocomplete: 'new-password' },
    email: { type: 'text', label: 'Email', required: true },
  };

  createEffect(() => {
    if (!data.taintedUsername)
      data.username = data.name.toLowerCase().replaceAll(/[^a-z0-9_]+/g, '_').replace(/^[^a-z]+/, '').replace(/[_]+$/, '');
  });

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    // Client-side validation
    if (data.password !== data.passwordConfirmation) {
      toast.error('The password and password confirmation fields must match.');
      return;
    }

    const output = await postRegister({
      name: data.name,
      email: data.email,
      username: data.username,
      password: data.password,
    });

    if (output.ok) {
      const state = here.state as { returnUrl?: string };
      toast.info('Success! Redirecting to the login page…');
      go('/login', { username: data.username, returnUrl: state.returnUrl });
    } else {
      toast.error(output.message);
    }
  };

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>Join</h1>

      <p>
        Register a new user account.
      </p>

      <form onSubmit={formSubmit} class='form-grid'>

        <FieldStack data={data} options={dataOptions} />

        <div />
        <div>
          <button type='submit' class='button paint-primary'>
            Join Now
          </button>
          <A href='/login' class='p-button'>Login</A>
        </div>

      </form>

    </section>
  );
}
