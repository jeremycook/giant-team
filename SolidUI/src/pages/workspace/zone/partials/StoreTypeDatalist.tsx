import { onCleanup } from "solid-js";
import { Portal } from "solid-js/web";
import { StoreType } from "../../../../bindings/GiantTeam.DatabaseDefinition.Models";
import { createId } from "../../../../helpers/htmlHelpers";
import { Do } from "../../../../widgets/Do";

const storeTypeList = [
    { value: StoreType.boolean, name: 'bool' },
    { value: StoreType.bytea, name: 'data' },
    { value: StoreType.date, name: 'date' },
    { value: StoreType.timestampTz, name: 'moment' },
    { value: StoreType.integer, name: 'int' },
    { value: StoreType.jsonb, name: 'jsonb' },
    { value: StoreType.text, name: 'text' },
    { value: StoreType.time, name: 'time' },
    { value: StoreType.uuid, name: 'uuid' },
    { value: StoreType.boolean, name: 'yes/no' },
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