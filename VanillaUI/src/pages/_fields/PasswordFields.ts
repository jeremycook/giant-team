import { h, HAttributes } from '../../helpers/h';

export interface PasswordFieldOptions extends HAttributes {
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

    return h('input', x => x.set({
        type: 'password',
        name: name,
        value: data[name],
        oninput: e => data[name] = (e.currentTarget as HTMLInputElement).value as string ?? '',
        autocomplete: autocomplete,
        required: true,
        ...attributes,
    }));
}
