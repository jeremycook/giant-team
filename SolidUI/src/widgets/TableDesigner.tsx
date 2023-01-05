import { For } from "solid-js";
import { Table, TableIndexType } from "../bindings/GiantTeam.DatabaseDefinition.Models";
import { removeItem } from "../helpers/arrayHelpers";
import { createId } from "../helpers/htmlHelpers";
import { DeleteOutlineIcon } from "../helpers/icons";
import { StoreTypeDatalist, storeTypeDatalistId } from "../pages/workspace/zone/partials/StoreTypeDatalist";

interface TableDesignerWidgetProps {
    table: Table;
    lockedColumnNames?: string[];
}

export function TableDesignerWidget({ table, lockedColumnNames }: TableDesignerWidgetProps) {

    return (
        <>
            <StoreTypeDatalist />
            <StoreTypeDatalist />
            <StoreTypeDatalist />

            <div class='form-grid'>

                <label for={createId('name')}>Name</label>
                <div>
                    <input id={createId('name')} value={table.name} oninput={e => table.name = (e.target as HTMLInputElement).value}
                        required
                        use:autofocus={true} />
                </div>

                <label for={createId('owner')}>Owner</label>
                <div>
                    <input id={createId('owner')} value={table.owner ?? undefined} oninput={e => table.owner = (e.target as HTMLInputElement).value} />
                </div>
                <details open class='col-span-2'>
                    <summary>Columns ({table.columns.length})</summary>
                    <table class='w-100% text-center'>
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Store Type</th>
                                <th>Default SQL</th>
                                <th>Computed SQL</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            <For each={table.columns}>{(column) => (
                                <tr>
                                    <td>{lockedColumnNames && lockedColumnNames.indexOf(column.name) > -1 ?
                                        <div class='p-input'>{column.name}</div> :
                                        <input value={column.name} oninput={e => column.name = (e.target as HTMLInputElement).value}
                                            required />
                                    }</td>
                                    <td>
                                        <div class='flex'>
                                            <input class='flex-grow'
                                                value={column.storeType}
                                                onchange={e => column.storeType = (e.target as HTMLInputElement).value}
                                                required
                                                list={storeTypeDatalistId} />
                                            <button type='button'
                                                class='input w-auto font-mono'
                                                onclick={e => column.isNullable = !column.isNullable}
                                                title={column.isNullable ? 'Null values are allowed. Click to disallow.' : 'Null values are not allowed. Click to allow.'}
                                            >
                                                {column.isNullable ? 'OPT' : 'REQ'}
                                            </button>
                                        </div>
                                    </td>
                                    <td>
                                        <input value={column.defaultValueSql ?? undefined} onchange={e => column.defaultValueSql = (e.target as HTMLInputElement).value} />
                                    </td>
                                    <td>
                                        <input value={column.computedColumnSql ?? undefined} onchange={e => column.computedColumnSql = (e.target as HTMLInputElement).value} />
                                    </td>
                                    <td>
                                        <button type='button' class='whitespace-nowrap button paint-red-600' onclick={() => removeItem(table.columns, column)}>
                                            <DeleteOutlineIcon />
                                            <span class='sr-only'>Remove</span>
                                        </button>
                                    </td>
                                </tr>
                            )}</For>
                            <tr>
                                <td colspan='100'>
                                    <button type='button' class='button' onclick={e => table.columns.push({
                                        position: table.columns.length + 1,
                                        name: '',
                                        storeType: '',
                                        isNullable: false,
                                        computedColumnSql: '',
                                        defaultValueSql: '',
                                    })}>
                                        Add Column
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </details>

                <details open class='col-span-2'>
                    <summary>Indexes ({table.indexes.length})</summary>
                    <table class='w-100% text-center'>
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Type</th>
                                <th>Columns</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            <For each={table.indexes}>{(index, row) => (
                                <tr>
                                    <td>
                                        <input value={index.name ?? undefined}
                                            oninput={e => index.name = (e.target as HTMLInputElement).value} />
                                    </td>
                                    <td class='text-left'>
                                        <select class='w-auto'
                                            onchange={e => index.indexType = parseInt((e.target as HTMLSelectElement).value) || TableIndexType.Index}>
                                            <option value={TableIndexType.Index} selected={index.indexType === TableIndexType.Index}>Index</option>
                                            <option value={TableIndexType.PrimaryKey} selected={index.indexType === TableIndexType.PrimaryKey}>Primary</option>
                                            <option value={TableIndexType.UniqueConstraint} selected={index.indexType === TableIndexType.UniqueConstraint}>Unique</option>
                                        </select>
                                    </td>
                                    <td class='vertical-middle'>
                                        <div>
                                            <For each={table.columns}>{column => (<>
                                                <label>
                                                    <input type='checkbox'
                                                        checked={index.columns.indexOf(column.name) > -1}
                                                        onchange={e => index.columns.indexOf(column.name) > -1 ?
                                                            removeItem(index.columns, column.name) :
                                                            index.columns.push(column.name)} />
                                                    {' ' + column.name}
                                                </label>
                                            </>)}</For>
                                        </div>
                                    </td>
                                    <td>
                                        <button type='button' class='button paint-red-600' onclick={() => removeItem(table.indexes, index)}>
                                            <DeleteOutlineIcon />
                                            <span class='sr-only'>Remove</span>
                                        </button>
                                    </td>
                                </tr>
                            )}</For>
                            <tr>
                                <td colspan='100'>
                                    <button type='button' class='button'
                                        onclick={e => table.indexes.push({
                                            name: '',
                                            indexType: TableIndexType.Index,
                                            columns: [],
                                        })}>
                                        Add Index
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </details>

            </div>
        </>
    );
}