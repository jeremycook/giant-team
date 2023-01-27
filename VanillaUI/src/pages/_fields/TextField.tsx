import { InputHTMLAttributes } from "../../helpers/jsx/jsx";

export interface TextFieldOptions extends InputHTMLAttributes<HTMLInputElement> {
    type: 'text';
    label: string;
}

export default function TextField({ name, options, data }: { name: string; options: TextFieldOptions; data: Record<string, any>; }) {
    const {
        type,
        label,
        ...attributes
    } = options;

    return (<>
        <input
            name={name}
            value={data[name]}
            oninput={e => data[name] = e.currentTarget.value ?? ''}
            autocomplete='off'
            {...attributes} />
    </>);
}
