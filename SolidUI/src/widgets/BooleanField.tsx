import { createId } from "../helpers/htmlHelpers";

export interface BooleanFieldOptions extends Record<string, string | number | boolean> {
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
                    id={createId(name + '-true')}
                    name={name}
                    checked={data[name] === true}
                    onchange={e => data[name] = true}
                    required
                    {...attributes} /> Yes
            </label>
            <label>
                <input
                    type='radio'
                    id={createId(name + '-false')}
                    name={name}
                    checked={data[name] !== true}
                    onchange={e => data[name] = false}
                    required
                    {...attributes} /> No
            </label>
        </div>
    </>);
}
