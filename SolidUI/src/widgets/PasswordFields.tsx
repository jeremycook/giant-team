import { createId } from "../helpers/htmlHelpers";

export interface PasswordFieldOptions extends Record<string, string | number | boolean> {
    type: 'password';
    label: string;
    autocomplete: 'current-password' | 'new-password';
}

export default function PasswordField({ name, options, data }: { name: string; options: PasswordFieldOptions; data: Record<string, any>; }) {
    const {
        type,
        label,
        ...attributes
    } = options;

    return (<>
        <input
            type='password'
            id={createId(name)}
            name={name}
            value={data[name]}
            oninput={e => data[name] = e.currentTarget.value ?? (typeof data[name] === 'string' ? '' : null)}
            required
            {...attributes} />
    </>);
}
