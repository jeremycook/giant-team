import { h, HAttributes } from '../../helpers/h';

export interface TextFieldOptions extends HAttributes {
    type: 'text';
    label: string;
}

export default function TextField({ name, options, data }: { name: string; options: TextFieldOptions; data: Record<string, any>; }) {
    const {
        type,
        label,
        oninput,
        autocomplete,
        ...attributes
    } = options;

    return h('input', x => x.set({
        name: name,
        value: data[name],
        oninput: e => {
            data[name] = (e.currentTarget as HTMLInputElement).value as string ?? '';
            if (typeof oninput === 'function') oninput(e);
        },
        autocomplete: autocomplete ?? 'off',
        ...attributes,
    }));
}
