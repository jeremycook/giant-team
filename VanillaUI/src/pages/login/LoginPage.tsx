import { postLogin } from "../../bindings/Authentication.Api.Controllers";
import { route } from "../Router";
import FieldStack, { FieldSetOptions } from "../_fields/FieldStack";
import CardLayout from "../_ui/CardLayout";
import { toast } from "../_ui/Toasts";
import { user } from "./user";

const dataOptions: FieldSetOptions = {
    username: { type: 'text', label: 'Username', required: true, autocomplete: 'username' },
    password: { type: 'password', label: 'Password', autocomplete: 'current-password' },
    remainLoggedIn: { type: 'boolean', label: 'Remember me' },
};

export default function LoginPage({ state }: { state: { returnUrl?: string } }) {

    const data = {
        username: '',
        password: '',
        remainLoggedIn: false,
    };

    return <CardLayout>
        <h1>Login</h1>
        <form onsubmit={async e => {
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
                if (returnUrl && route.isLocal(returnUrl)) {
                    route.redirect(returnUrl);
                }
                else {
                    route.redirect('/profile');
                }

                return; //
            } else {
                toast.error(output.message);
            }
        }}>

            <FieldStack options={dataOptions} data={data} />

            <button>Login</button>
        </form>
    </CardLayout>
}