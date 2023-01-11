import { Show } from 'solid-js';
import { isAuthenticated, refreshSession, session } from '../utils/session';
import { InfoIcon } from '../helpers/icons';
import { FieldStack, FieldSetOptions } from '../widgets/FieldStack';
import { createMutable } from 'solid-js/store';
import { Anchor } from '../partials/Anchor';
import { isLocalUrl } from '../helpers/urlHelpers';
import { postLogin } from '../bindings/GiantTeam.Authentication.Api.Controllers';
import { useLocation, useNavigate } from '@solidjs/router';
import { toast } from '../partials/Toasts';
import { CardLayout } from '../partials/CardLayout';

const dataOptions: FieldSetOptions = {
    username: { type: 'text', label: 'Username', required: true, autocomplete: 'username' },
    password: { type: 'password', label: 'Password', autocomplete: 'current-password' },
    remainLoggedIn: { type: 'boolean', label: 'Remember me' },
};

export default function LoginPage() {
    const here = useLocation<{ username: string, returnUrl: string }>();
    const navigate = useNavigate();

    const data = createMutable({
        username: here.state?.username ?? '',
        password: '',
        remainLoggedIn: false,
    });

    const returnUrl = () => {
        if (here.state?.returnUrl) {
            const url = new URL(here.state?.returnUrl, location.href);
            if (isLocalUrl(url) && !url.pathname.endsWith('/login'))
                return url.toString();
        }

        // Fallback
        return '/my';
    };

    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();
        const form = e.target as HTMLFormElement;

        const output = await postLogin({
            username: form.username.value,
            password: form.password.value,
            elevated: true, // TODO: toggle
            remainLoggedIn: form.remainLoggedIn.checked,
        });

        if (output.ok) {
            toast.success('Logging you in…');

            // Refresh the session
            await refreshSession()

            const url = returnUrl();
            console.debug(`Redirecting from ${location.href} to ${url}.`)
            navigate(url);
            return;
        } else {
            toast.error(output.message);
        }
    };

    return (
        <CardLayout>

            <h1>Login</h1>

            <Show when={isAuthenticated()}>
                <p class='text-info' role='alert'>
                    <InfoIcon class='animate-bounce-in' />{' '}
                    FYI: You are currently logged in as <Anchor href='/profile'>{session.username}</Anchor>.
                    <Show when={returnUrl()}>
                        {' '}<Anchor href={returnUrl()!} class='underline'>Click here to go back</Anchor>.
                    </Show>
                </p>
            </Show>

            <form onSubmit={onSubmitForm} class='form-grid'>

                <FieldStack data={data} options={dataOptions} />

                <div />
                <div>
                    <button type='submit' class='button paint-primary'>
                        Login
                    </button>
                    <Anchor href='/join' class='p-button'>Join</Anchor>
                </div>

            </form>

        </CardLayout>
    );
}
