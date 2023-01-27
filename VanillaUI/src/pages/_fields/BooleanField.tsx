import { InputHTMLAttributes } from "../../helpers/jsx/jsx";

export interface BooleanFieldOptions extends InputHTMLAttributes<HTMLInputElement> {
    type: 'boolean';
    label: string;
}

export default function BooleanField({ name, options, data }: { name: string; options: BooleanFieldOptions; data: Record<string, any>; }) {
    const {
        type,
        label,
        ...attributes
    } = options;

    return (<>
        <div>
            <label>
                <input
                    type='radio'
                    name={name}
                    checked={data[name] === true}
                    onchange={e => data[name] = true}
                    required
                    {...attributes} /> Yes
            </label>
            <label>
                <input
                    type='radio'
                    name={name}
                    checked={data[name] !== true}
                    onchange={e => data[name] = false}
                    required
                    {...attributes} /> No
            </label>
        </div>
    </>);
}
