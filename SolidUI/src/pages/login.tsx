import { Show } from 'solid-js';
import { user } from '../utils/session';
import { InfoIcon } from '../helpers/icons';
import { FieldStack, FieldSetOptions } from '../widgets/FieldStack';
import { createMutable } from 'solid-js/store';
import { Anchor } from '../partials/Anchor';
import { createUrl, isLocalUrl, relativeHref } from '../helpers/urlHelpers';
import { postLogin } from '../bindings/GiantTeam.Authentication.Api.Controllers';
import { useLocation, useNavigate } from '@solidjs/router';
import { toast } from '../partials/Toasts';
import { CardLayout } from '../partials/CardLayout';

const dataOptions: FieldSetOptions = {
    username: { type: 'text', label: 'Username', required: true, autocomplete: 'username' },
    password: { type: 'password', label: 'Password', autocomplete: 'current-password' },
    remainLoggedIn: { type: 'boolean', label: 'Remember me' },
};

export function Login(props: { username?: string, returnUrl?: string | false }) {
    const navigate = useNavigate();

    const data = createMutable({
        username: props.username ?? '',
        password: '',
        remainLoggedIn: false,
    });


    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        const output = await postLogin({
            username: data.username,
            password: data.password,
            remainLoggedIn: data.remainLoggedIn,
            elevated: true,
        });

        if (output.ok) {
            toast.success(`Welcome back ${data.username}!`);

            await user.refresh()

            if (typeof props.returnUrl === 'string' && isLocalUrl(props.returnUrl)) {
                console.debug(`Redirecting from ${location.href} to ${props.returnUrl}.`)
                navigate(props.returnUrl);
            }
            
            return;
        } else {
            toast.error(output.message);
        }
    };

    return <>
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
    </>
}

export default function LoginPage() {
    const here = useLocation<{ username: string, returnUrl: string }>();

    const returnUrl = () => {
        const returnUrl = here.state?.returnUrl ?? here.query.returnUrl;

        if (returnUrl && isLocalUrl(returnUrl)) {
            const url = createUrl(returnUrl);
            if (isLocalUrl(url) && !url.pathname.endsWith('/login'))
                return relativeHref(url);
        }

        // Fallback
        return '/my';
    };

    return <CardLayout>
        <h1>Login</h1>

        <Show when={user.isAuthenticated}>
            <p class='text-info' role='alert'>
                <InfoIcon class='animate-bounce-in' />{' '}
                FYI: You are currently logged in as <Anchor href='/profile'>{user.username}</Anchor>.
                <Show when={returnUrl()}>
                    {' '}<Anchor href={returnUrl()!} class='underline'>Click here to go back</Anchor>.
                </Show>
            </p>
        </Show>

        <Login username={here.state?.username} returnUrl={returnUrl()} />
    </CardLayout>
}