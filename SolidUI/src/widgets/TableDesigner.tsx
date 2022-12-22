import { For } from "solid-js";
import { StoreType, Table, TableIndexType } from "../api/GiantTeam";
import { removeItem } from "../helpers/arrayHelpers";
import { createId } from "../helpers/htmlHelpers";
import { DeleteOutlineIcon } from "../helpers/icons";
import { snakeCase } from "../helpers/textHelpers";

interface TableDesignerWidgetProps {
    table: Table;
}

export function TableDesignerWidget({ table }: TableDesignerWidgetProps) {

    return (
        <>
            <datalist id={createId('datalist')}>
                <option value={StoreType.boolean}>{StoreType.boolean}</option>
                <option value={StoreType.bytea}>{StoreType.bytea}</option>
                <option value={StoreType.bytea}>data</option>
                <option value={StoreType.date}>{StoreType.date}</option>
                <option value={StoreType.timestampTz}>date/time</option>
                <option value={StoreType.integer}>{StoreType.integer}</option>
                <option value={StoreType.jsonb}>{StoreType.jsonb}</option>
                <option value={StoreType.text}>{StoreType.text}</option>
                <option value={StoreType.time}>{StoreType.time}</option>
                <option value={StoreType.timestampTz}>{StoreType.timestampTz}</option>
                <option value={StoreType.boolean}>true/false</option>
                <option value={StoreType.uuid}>{StoreType.uuid}</option>
                <option value={StoreType.boolean}>yes/no</option>
            </datalist>

            <div class='form-grid'>

                <label for={createId('name')}>Name</label>
                <div>
                    <input id={createId('name')} value={table.name} oninput={e => table.name = (e.target as HTMLInputElement).value}
                        required />
                </div>

                <label for={createId('owner')}>Owner</label>
                <div>
                    <input id={createId('owner')} value={table.owner} oninput={e => table.owner = (e.target as HTMLInputElement).value} />
                </div>

                <details open class='col-span-2'>
                    <summary>Columns ({table.columns.length})</summary>
                    <table class='w-100% text-center'>
                        <thead>
                            <tr>
                                <th title='Default position'>#</th>
                                <th>Name</th>
                                <th>Store Type</th>
                                <th>Default Value SQL</th>
                                <th>Computed Column SQL</th>
                            </tr>
                        </thead>
                        <tbody>
                            <For each={table.columns}>{(column) => (
                                <tr>
                                    <td>
                                        {column.position}
                                        {/* <input value={column.position}
                                            oninput={e => column.position = parseInt((e.target as HTMLInputElement).value) || 0}
                                            required /> */}
                                    </td>
                                    <td>
                                        <input value={column.name} oninput={e => column.name = (e.target as HTMLInputElement).value}
                                            required />
                                    </td>
                                    <td>
                                        <div class='flex'>
                                            <input class='flex-grow'
                                                value={column.storeType}
                                                onchange={e => column.storeType = (e.target as HTMLInputElement).value}
                                                required
                                                list={createId('datalist')} />
                                            <label class='input w-auto'>
                                                <input type='checkbox'
                                                    checked={column.isNullable}
                                                    onchange={e => column.isNullable = (e.target as HTMLInputElement).checked}
                                                    title={column.isNullable ? 'Null values are allowed. Uncheck to disallow.' : 'Null value are not allowed. Check to allow.'}
                                                /><span class='sr-only'>Nullable</span>
                                            </label>
                                        </div>
                                    </td>
                                    <td>
                                        <input value={column.defaultValueSql} onchange={e => column.defaultValueSql = (e.target as HTMLInputElement).value} />
                                    </td>
                                    <td>
                                        <input value={column.computedColumnSql} onchange={e => column.computedColumnSql = (e.target as HTMLInputElement).value} />
                                    </td>
                                    <td>
                                        <button type='button' class='whitespace-nowrap button p-1 paint-red-600' onclick={() => removeItem(table.columns, column)}>
                                            <DeleteOutlineIcon />
                                            <span class='sr-only'>Remove</span>
                                        </button>
                                    </td>
                                </tr>
                            )}</For>
                            <tr>
                                <td colspan='6'>
                                    <button type='button' class='button p-1' onclick={e => table.columns.push({
                                        position: table.columns.length + 1,
                                        name: '',
                                        storeType: '',
                                        isNullable: true,
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
                                <th>Columns</th>
                            </tr>
                        </thead>
                        <tbody>
                            <For each={table.indexes}>{(index, row) => (
                                <tr>
                                    <td>
                                        <div class='flex gap-1'>
                                            <input value={index.name} oninput={e => index.name = (e.target as HTMLInputElement).value}
                                                required />
                                            <select onchange={e => index.indexType = parseInt((e.target as HTMLSelectElement).value) || TableIndexType.Index}>
                                                <option value={TableIndexType.Index} selected={index.indexType === TableIndexType.Index}>Index</option>
                                                <option value={TableIndexType.PrimaryKey} selected={index.indexType === TableIndexType.PrimaryKey}>Primary</option>
                                                <option value={TableIndexType.UniqueConstraint} selected={index.indexType === TableIndexType.UniqueConstraint}>Unique</option>
                                            </select>
                                        </div>
                                    </td>
                                    <td class='text-left'>
                                        <div class='flex flex-wrap gap-1 children:b children:rounded children:p-1'>
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
                                        <button type='button' class='button p-1 paint-red-600' onclick={() => removeItem(table.indexes, index)}>
                                            <DeleteOutlineIcon />
                                            <span class='sr-only'>Remove</span>
                                        </button>
                                    </td>
                                </tr>
                            )}</For>
                            <tr>
                                <td colspan='3'>
                                    <button type='button' class='button p-1'
                                        onclick={e => table.indexes.push({
                                            name: `ix_${snakeCase(table.name)}_${table.indexes.length + 1}`,
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