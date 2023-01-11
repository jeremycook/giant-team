import { createEffect } from 'solid-js';
import { createMutable } from 'solid-js/store';
import { Anchor } from '../partials/Anchor';
import { toast } from '../partials/Toasts';
import { FieldSetOptions, FieldStack } from '../widgets/FieldStack';
import { postRegister } from '../bindings/GiantTeam.Authentication.Api.Controllers';
import { useLocation } from '@solidjs/router';
import { useGo } from '../helpers/httpHelpers';
import { CardLayout } from '../partials/CardLayout';

export const pageSettings = {
    name: 'Join',
}

export default function JoinPage() {
    const here = useLocation<{ returnUrl: string }>();
    const go = useGo();

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
        password: { type: 'password', label: 'Password', autocomplete: 'new-password', minLength: 12 },
        passwordConfirmation: { type: 'password', label: 'Password Confirmation', autocomplete: 'new-password' },
        email: { type: 'text', label: 'Email', required: true },
    };

    createEffect(() => {
        if (!data.taintedUsername)
            data.username = data.name.toLowerCase().replaceAll(/[^a-z0-9_]+/g, '_').replace(/^[^a-z]+/, '').replace(/[_]+$/, '');
    });

    const onSubmitForm = async (e: SubmitEvent) => {
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
            toast.info('Success!');
            go('/login', { username: data.username, returnUrl: here.state?.returnUrl });
        } else {
            toast.error(output.message);
        }
    };

    return (
        <CardLayout>

            <h1>Join</h1>

            <p>
                Register a new user account.
            </p>

            <form onSubmit={onSubmitForm} class='form-grid'>

                <FieldStack data={data} options={dataOptions} />

                <div />
                <div>
                    <button type='submit' class='button paint-primary'>
                        Join Now
                    </button>
                    <Anchor href='/login' class='p-button'>Login</Anchor>
                </div>

            </form>

        </CardLayout>
    );
}
