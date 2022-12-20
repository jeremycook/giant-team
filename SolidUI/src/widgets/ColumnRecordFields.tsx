import { For, Match, Switch } from "solid-js";
import { createId } from "../helpers/htmlHelpers";
import { MetaColumn } from "./SmartTable";

export interface ColumnRecordFieldsProps {
    fields: { column: MetaColumn, value: any }[],
}

export default function ColumnRecordFields(props: ColumnRecordFieldsProps) {
    return (
        <For each={props.fields.map(f => ({ ...f, id: createId(f.column.name) }))}>{({ column, value, id }) => (
            <>
                <label for={id}>{column.name}</label>
                <div>
                    <Switch fallback={'Unsupported type: ' + column.dataType}>
                        <Match when={column.dataType === 'text'}>
                            <textarea id={id} value={typeof value === 'string' ? value : ''} rows='1' />
                        </Match>
                        <Match when={column.dataType === 'uuid'}>
                            <input id={id} value={typeof value === 'string' ? value : ''} />
                        </Match>
                    </Switch>
                </div>
            </>
        )}</For>
    )
}