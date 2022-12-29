import { createId } from "../helpers/htmlHelpers";
import { PropMeta } from "./FormFields";


export default function TextInput({ name, prop, data }: { name: string; prop: PropMeta; data: Record<string, any>; }) {
    return (<>
        <input id={createId(name)}
            name={name}
            value={data[name]}
            oninput={e => data[name] = e.currentTarget.value ?? (typeof data[name] === 'string' ? '' : null)}
            autocomplete='off'
            required={prop.required} />
    </>);
}
