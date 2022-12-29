import { JSX } from "solid-js/web/types/jsx";
import { createId } from "../helpers/htmlHelpers";

export interface PasswordFieldOptions extends JSX.InputHTMLAttributes<HTMLInputElement> {
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
            id={createId(name)}
            name={name}
            value={data[name]}
            oninput={e => data[name] = e.currentTarget.value ?? ''}
            autocomplete={autocomplete}
            required
            {...attributes} />
    </>);
}
