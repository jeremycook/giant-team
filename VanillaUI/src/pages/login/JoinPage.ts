import { h } from '../../helpers/h';
import { postRegister } from '../../bindings/Authentication.Api.Controllers';
import { route } from '../Router';
import { toast } from '../_ui/Toast';
import CardLayout from '../_ui/CardLayout';
import FieldStack, { FieldSetOptions } from '../_fields/FieldStack';

export default function JoinPage() {
    let touchedUsername = false;

    const state = route.state as { username?: string, returnUrl?: string };
    const data = {
        name: '',
        username: state.username ?? '',
        password: '',
        passwordConfirmation: '',
        email: '',
    };

    const dataOptions: FieldSetOptions = {
        name: {
            type: 'text', label: 'Name', required: true, autofocus: true, oninput: (e) => {
                if (!touchedUsername) {
                    const ct = e.currentTarget as HTMLInputElement;
                    ct.form!.username.value = ct.value?.toLowerCase().replaceAll(/[^a-z0-9_]+/g, '_').replace(/^[^a-z]+/, '').replace(/[_]+$/, '');
                }
            }
        },
        username: { type: 'text', label: 'Username', required: true, autocomplete: 'new-username', onfocus: () => touchedUsername = true },
        password: { type: 'password', label: 'Password', autocomplete: 'new-password', minLength: 12 },
        passwordConfirmation: { type: 'password', label: 'Password Confirmation', autocomplete: 'new-password' },
        email: { type: 'text', label: 'Email', required: true },
    };

    const onsubmit = async (e: Event) => {
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

    return CardLayout(
        h('h1', 'Join'),
        h('p', 'Register a new user account.'),

        h('form.form-grid', x => x.set({ onsubmit }),
            ...FieldStack({ data, options: dataOptions }),

            h('div'),
            h('.flex.gap-50',
                h('button.button', 'Join Now'),
                h('a.button', x => x.set({ href: '/login' }), 'Login'),
            ),
        ),
    )
}
