import { onCleanup } from "solid-js";
import { Portal } from "solid-js/web";
import { StoreType } from "../bindings/GiantTeam.DatabaseDefinition.Models";
import { createId } from "../helpers/htmlHelpers";
import { Do } from "./Do";

const storeTypeList = [
    { value: StoreType.Boolean, name: 'bool' },
    { value: StoreType.Bytea, name: 'data' },
    { value: StoreType.Date, name: 'date' },
    { value: StoreType.TimestampTz, name: 'moment' },
    { value: StoreType.Integer, name: 'int' },
    { value: StoreType.Jsonb, name: 'jsonb' },
    { value: StoreType.Text, name: 'text' },
    { value: StoreType.Time, name: 'time' },
    { value: StoreType.Uuid, name: 'uuid' },
    { value: StoreType.Boolean, name: 'yes/no' },
];

export const storeTypeDatalistId = createId('datalist');

let rendered = false;

export function StoreTypeDatalist(props: Record<string, unknown>) {
    onCleanup(() => rendered = false);

    return rendered ? null : (<>
        <Do>{() => rendered = true}</Do>
        <Portal>
            <datalist id={storeTypeDatalistId} {...props}>
                {storeTypeList.map(o => <option value={o.value}>{o.name}</option>)}
            </datalist>
        </Portal>
    </>);
}