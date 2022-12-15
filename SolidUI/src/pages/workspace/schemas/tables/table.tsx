import { createStore } from 'solid-js/store';
import { useParams, useSearchParams } from '@solidjs/router';
import { batch, createEffect, createResource, createSignal, Show } from 'solid-js';
import { FetchRecordsInput, Sort } from '../../../../api/GiantTeam';
import { postFetchRecords } from '../../../../api/GiantTeam.Data.Api';
import { titleSetter } from '../../../../title';
import { Data, Meta, MetaColumn } from '../../../../widgets/SmartTable';
import SmartTable from '../../../../widgets/SmartTable';

export default function WorkspacePage() {
    titleSetter('Table');

    const [data, setData] = createStore<Data>({
        columns: [],
        records: []
    });

    const [meta, setMeta] = createStore<Meta>({
        columns: {},
    });

    const params = useParams();
    const [search] = useSearchParams();

    const [resource] = createResource(() => {
        return {
            database: params.workspace,
            schema: search.schema,
            table: search.table,
            columns: Object.values(meta.columns)
                // TODO: .filter(c => c.visible)
                .map(c => ({
                    name: c.name,
                    sort: c.sort ?? Sort.Unsorted,
                    position: c.position,
                })),
            // filters: params.filters,
            // skip: params.skip,
            // take: params.take
        };
    }, async (input: FetchRecordsInput) => {
        const output = await postFetchRecords(input)
        return output;
    });

    const ok = () => resource()?.ok == true;
    const message = () => resource()?.message || null;
    const [activeRecord, activeRecordSetter] = createSignal<any[] | null>(null);

    const toggleActiveRecord = (record: any[]) => record === activeRecord() ? activeRecordSetter(null) : activeRecordSetter(record);

    createEffect(() => {
        batch(() => {

            const columns = resource()?.data?.columns;
            if (columns) {
                setData('columns', columns.map(c => c.name));

                columns.forEach((column, i) =>
                    setMeta('columns', column.name, (state?: MetaColumn) => ({
                        name: column.name,
                        dataType: column.dataType,
                        nullable: column.nullable,
                        sort: state?.sort ?? Sort.Unsorted,
                        visible: state?.visible ?? true, // default to visible
                        position: state?.position ?? i, // default to initial position
                    }))
                );
            }

            const records = resource()?.data?.records;
            if (records) {
                // TODO: Diff based on primary key or unique columns if known?
                setData('records', []);
                setData('records', records);
            }

        });
    });

    createEffect(() => titleSetter(params.table ?? 'Table'));

    return (
        <section class='pr-1'>

            <Show when={resource.loading}>Loading…</Show>

            <Show when={message()}>
                <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
                    {message()}
                </p>
            </Show>

            <Show when={activeRecord()}>
                {JSON.stringify(activeRecord())}
            </Show>

            <Show when={data.records.length > 0}>

                <SmartTable
                    data={data}
                    meta={meta}
                    setMeta={setMeta}
                    headLeader={() => <th></th>}
                    rowLeader={(record, row) => <td class='p-0'>
                        <div class='w-100% h-100%'>
                            <button class='block w-100% h-100%' onclick={() => toggleActiveRecord(record!)}>
                                {row() + 1}
                            </button>
                        </div>
                    </td>}
                ></SmartTable>

                {/* <table>
                        <thead>
                            <tr>
                                <th></th>
                                <For each={data()?.columns}>{column =>
                                    <th
                                        onClick={() => setOrder([{ column: column.name, desc: order().findIndex(val => val.column === column.name && val.desc !== true) > -1 }])}
                                        class='cursor-pointer'
                                    >{column.name}</th>
                                }</For>
                            </tr>
                        </thead>
                        <tbody>
                            <For each={data()?.records}>{(record, row) =>
                                <tr>
                                    <td class='bg-sky-1'>{row() + 1}</td>
                                    <For each={data()?.columns}>{(col, i) =>
                                        <td classList={{ 'text-left': col.dataType === 'text' }}>{record[i()]}</td>}
                                    </For>
                                </tr>
                            }</For>
                        </tbody>
                    </table> */}

            </Show>

        </section>
    );
}
