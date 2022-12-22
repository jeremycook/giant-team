import { useParams, useSearchParams } from "@solidjs/router";
import { For } from "solid-js";
import { createMutable, unwrap } from "solid-js/store";
import { StoreType, Table } from "../../api/GiantTeam";
import { postCreateTable } from "../../api/GiantTeam.Data.Api";
import { removeItem } from "../../helpers/arrayHelpers";
import { createId } from "../../helpers/htmlHelpers";
import { DeleteOutlineIcon } from "../../helpers/icons";
import { snakeCase } from "../../helpers/textHelpers";
import { setTitle } from "../../utils/page"

export default function CreateTablePage() {
    setTitle('Create a Table');

    const params = useParams();
    const [search] = useSearchParams();

    const info = {
        workspace: params.workspace as string,
        schema: search.schema as string,
    };

    const table = createMutable<Table>({
        name: 'Blog Post',
        owner: '',
        columns: [{
            name: 'Id',
            storeType: StoreType.uuid,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: 'gen_random_uuid()',
        },
        {
            name: 'Title',
            storeType: StoreType.text,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: '',
        },
        {
            name: 'Body',
            storeType: StoreType.text, // TODO: 'html'
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: '',
        },
        {
            name: 'Slug',
            storeType: StoreType.text,
            isNullable: false,
            computedColumnSql: `trim(regexp_replace(Title, '[^\w]+', '-', 'g'), '-')`,
            defaultValueSql: '',
        },
        {
            name: 'Created',
            storeType: StoreType.timestampTz,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: `(CURRENT_TIMESTAMP AT TIME ZONE 'UTC')`,
        },
        {
            name: 'Created By',
            storeType: StoreType.text,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: 'CURRENT_USER',
        }],
        indexes: [{
            name: 'ix_Blog_Post_Slug',
            isUnique: true,
            columns: ['Slug'],
        }],
        uniqueConstraints: [{
            name: 'pk_Blog_Post',
            isPrimaryKey: true,
            columns: ['Id'],
        }],
    });

    const onsubmitform = async (e: SubmitEvent) => {
        e.preventDefault();

        const input = {
            databaseName: info.workspace,
            schemaName: info.schema,
            table: unwrap(table),
        };

        const response = await postCreateTable(input);

        if (response.ok) {
            alert('Table created!');
        }
        else {
            alert('Error: ' + response.message);
        }
    };

    return (
        <section class='card card md:w-900px max-w-100% md:mx-auto'>
            <h1>Create a Table</h1>

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

            <form onsubmit={onsubmitform} class='form-grid'>

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
                    <summary>Columns</summary>
                    <table class='w-100% text-center'>
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Store Type</th>
                                <th>Nullable</th>
                                <th>Default Value SQL</th>
                                <th>Computed Column SQL</th>
                            </tr>
                        </thead>
                        <tbody>
                            <For each={table.columns}>{(column) => (
                                <tr>
                                    <td>
                                        <input value={column.name} oninput={e => column.name = (e.target as HTMLInputElement).value}
                                            required />
                                    </td>
                                    <td>
                                        <input value={column.storeType} onchange={e => column.storeType = (e.target as HTMLInputElement).value}
                                            required
                                            list={createId('datalist')} />
                                    </td>
                                    <td class='text-center'>
                                        <input type='checkbox' checked={column.isNullable} onchange={e => column.isNullable = (e.target as HTMLInputElement).checked} />
                                    </td>
                                    <td>
                                        <input value={column.defaultValueSql} onchange={e => column.defaultValueSql = (e.target as HTMLInputElement).value} />
                                    </td>
                                    <td>
                                        <input value={column.computedColumnSql} onchange={e => column.computedColumnSql = (e.target as HTMLInputElement).value} />
                                    </td>
                                    <td>
                                        <button type='button' class='whitespace-nowrap button p-1 paint-red-600' onclick={() => removeItem(table.columns, column)}>
                                            <DeleteOutlineIcon /> Remove
                                        </button>
                                    </td>
                                </tr>
                            )}</For>
                            <tr>
                                <td colspan='6'>
                                    <button type='button' class='button p-1' onclick={e => table.columns.push({
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
                    <summary>Indexes</summary>
                    <table class='w-100% table-fixed text-center'>
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
                                        <div class='flex flex-col gap-1'>
                                            <input value={index.name} oninput={e => index.name = (e.target as HTMLInputElement).value}
                                                required />
                                            <div class='flex justify-between gap-1'>
                                                <label class='inline-block p-1 b rounded'>
                                                    <input type='checkbox' checked={index.isUnique} onchange={e => index.isUnique = (e.target as HTMLInputElement).checked} />
                                                    {' '}Unique
                                                </label>
                                                <button type='button' class='button p-1 paint-red-600' onclick={() => removeItem(table.indexes, index)}>
                                                    <DeleteOutlineIcon />
                                                    Remove
                                                </button>
                                            </div>
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
                                </tr>
                            )}</For>
                            <tr>
                                <td colspan='2'>
                                    <button type='button' class='button p-1'
                                        onclick={e => table.indexes.push({
                                            name: `ux_${snakeCase(table.name)}_${table.indexes.length + 1}`,
                                            isUnique: false,
                                            columns: [''],
                                        })}>
                                        Add Index
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </details>

                <details open class='col-span-2'>
                    <summary>Unique Constraints</summary>
                    <table class='w-100% table-fixed text-center'>
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Columns</th>
                            </tr>
                        </thead>
                        <tbody>
                            <For each={table.uniqueConstraints}>{(uniqueConstraint, row) => (
                                <tr>
                                    <td>
                                        <div class='flex flex-col gap-1'>
                                            <input value={uniqueConstraint.name} oninput={e => uniqueConstraint.name = (e.target as HTMLInputElement).value}
                                                required />
                                            <div class='flex justify-between gap-1'>
                                                <label class='inline-block p-1 b rounded'>
                                                    <input type='checkbox' checked={uniqueConstraint.isPrimaryKey} onchange={e => uniqueConstraint.isPrimaryKey = (e.target as HTMLInputElement).checked} />
                                                    {' '}Primary Key
                                                </label>
                                                <button type='button' class='button p-1 paint-red-600' onclick={() => removeItem(table.uniqueConstraints, uniqueConstraint)}>
                                                    <DeleteOutlineIcon />
                                                    Remove
                                                </button>
                                            </div>
                                        </div>
                                    </td>
                                    <td class='text-left'>
                                        <div class='flex flex-wrap gap-1 children:b children:rounded children:p-1'>
                                            <For each={table.columns}>{column => (<>
                                                <label>
                                                    <input type='checkbox'
                                                        checked={uniqueConstraint.columns.indexOf(column.name) > -1}
                                                        onchange={e => uniqueConstraint.columns.indexOf(column.name) > -1 ?
                                                            removeItem(uniqueConstraint.columns, column.name) :
                                                            uniqueConstraint.columns.push(column.name)} />
                                                    {' ' + column.name}
                                                </label>
                                            </>)}</For>
                                        </div>
                                    </td>
                                </tr>
                            )}</For>
                            <tr>
                                <td colspan='2'>
                                    <button type='button' class='button p-1'
                                        onclick={e => table.uniqueConstraints.push({
                                            name: `ux_${snakeCase(table.name)}_${table.uniqueConstraints.length + 1}`,
                                            isPrimaryKey: false,
                                            columns: [''],
                                        })}>
                                        Add Unique Constraint
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </details>

                <div>
                    <button class='button'>Create Table</button>
                </div>

            </form>
        </section>
    )
}