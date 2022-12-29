import { Dynamic } from "solid-js/web";
import { createId } from "../helpers/htmlHelpers";
import TextInput from "./TextInput";

export interface PropMeta {
    type: 'text';
    label: string;
    required: boolean;
}

const lookup = {
    text: TextInput,
}

export function FormFields({ data, meta }: { data: Record<string, any>, meta: Record<string, PropMeta> }) {
    console.log(meta)
    return (<>
        {Object.entries(meta).map(([name, prop]) => (<>
            <label for={createId(name)}>{prop.label ?? name}</label>
            <Dynamic component={lookup[prop.type]} {...{ name, prop, data, meta }} />
        </>))}
    </>)
}
