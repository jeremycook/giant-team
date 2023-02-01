import { postLogin } from "../../bindings/Authentication.Api.Controllers";
import { h } from '../../helpers/h';
import { http } from '../../helpers/http';
import { route } from "../Router";
import FieldStack, { FieldSetOptions } from "../_fields/FieldStack";
import CardLayout from "../_ui/CardLayout";
import { toast } from "../_ui/Toasts";
import { user } from "./user";

const dataOptions: FieldSetOptions = {
    username: { type: 'text', label: 'Username', required: true, autocomplete: 'username', autofocus: true },
    password: { type: 'password', label: 'Password', autocomplete: 'current-password' },
    remainLoggedIn: { type: 'boolean', label: 'Remember me' },
};

export default function LoginPage() {

    const state = route.state as { username?: string, returnUrl?: string };
    const data = {
        username: state.username ?? '',
        password: '',
        remainLoggedIn: false,
    };

    async function onsubmit(e: Event) {
        e.preventDefault();

        const output = await postLogin({
            username: data.username,
            password: data.password,
            remainLoggedIn: data.remainLoggedIn,
            elevated: true,
        });

        if (output.ok) {
            toast.success(`Welcome back ${data.username}!`);

            await user.refresh();

            const returnUrl = state.returnUrl;
            if (returnUrl && http.isLocal(returnUrl)) {
                route.redirect(returnUrl);
            }
            else {
                route.redirect('/my');
            }
        } else {
            toast.error(output.message);
        }
    }

    return CardLayout(
        h('h1', 'Login'),
        h('p', 'Login with your user account.'),

        h('form.form-grid', x => x.set({ onsubmit }),
            ...FieldStack({ data, options: dataOptions }),

            h('div'),
            h('.flex.gap-50',
                h('button.button', 'Login'),
                h('a.button', x => x.set({ href: '/join' }), 'Join'),
            ),
        ),
    )
}