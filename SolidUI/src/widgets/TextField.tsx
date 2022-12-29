import { JSX } from "solid-js";
import { createId } from "../helpers/htmlHelpers";

export interface TextFieldOptions extends JSX.InputHTMLAttributes<HTMLInputElement> {
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
            id={createId(name)}
            name={name}
            value={data[name]}
            oninput={e => data[name] = e.currentTarget.value ?? ''}
            autocomplete='off'
            {...attributes} />
    </>);
}
