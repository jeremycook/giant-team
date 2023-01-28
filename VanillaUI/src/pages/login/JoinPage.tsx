import { postRegister } from '../../bindings/Authentication.Api.Controllers';
import { route } from '../Router';
import FieldStack, { FieldSetOptions } from '../_fields/FieldStack';
import CardLayout from '../_ui/CardLayout';
import { toast } from '../_ui/Toasts';

export default function JoinPage({ state }: { state: { username?: string, returnUrl?: string } }) {
    let touchedUsername = false;

    const data = {
        name: '',
        username: state.username ?? '',
        password: '',
        passwordConfirmation: '',
        email: '',
    };

    const dataOptions: FieldSetOptions = {
        name: {
            type: 'text', label: 'Name', required: true, oninput: (e) => {
                if (!touchedUsername) e.currentTarget.form!.username.value = e.currentTarget.value?.toLowerCase().replaceAll(/[^a-z0-9_]+/g, '_').replace(/^[^a-z]+/, '').replace(/[_]+$/, '');
            }
        },
        username: { type: 'text', label: 'Username', required: true, autocomplete: 'new-username', onfocus: () => touchedUsername = true },
        password: { type: 'password', label: 'Password', autocomplete: 'new-password', minLength: 12 },
        passwordConfirmation: { type: 'password', label: 'Password Confirmation', autocomplete: 'new-password' },
        email: { type: 'text', label: 'Email', required: true },
    };

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
            route.redirect('/login', { username: data.username, returnUrl: state.returnUrl });
            return;
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
                <div class='flex gap-50'>
                    <button class='button'>
                        Join Now
                    </button>
                    <a class='button' href='/login'>Login</a>
                </div>

            </form>

        </CardLayout>
    );
}
