import { InputHTMLAttributes } from "../../helpers/jsx/jsx";

export interface PasswordFieldOptions extends InputHTMLAttributes<HTMLInputElement> {
    type: 'password';
    label: string;
    autocomplete: 'current-password' | 'new-password';
}

export default function PasswordField({ name, options, data }: { name: string; options: PasswordFieldOptions; data: Record<string, any>; }) {
    const {
        type,
        label,
        autocomplete,
        ...attributes
    } = options;

    return (<>
        <input
            type='password'
            name={name}
            value={data[name]}
            oninput={e => data[name] = e.currentTarget.value ?? ''}
            autocomplete={autocomplete}
            required
            {...attributes} />
    </>);
}
