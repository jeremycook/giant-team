import { createStore, unwrap } from 'solid-js/store';
import { batch, createEffect, createResource, createSignal, Show } from 'solid-js';
import { FetchRecordsInput, Sort } from '../../../../api/GiantTeam';
import { postFetchRecords } from '../../../../api/GiantTeam.Data.Api';
import { titleSetter } from '../../../../title';
import { Data, Meta, MetaColumn } from '../../../../widgets/SmartTable';
import SmartTable from '../../../../widgets/SmartTable';
import { Portal } from 'solid-js/web';
import { routeValues } from '../../../../utils/routing';
import { debug } from '../../../../utils/logging';

export default function WorkspacePage() {
    titleSetter('Table');

    const [data, setData] = createStore<Data>({
        columns: [],
        records: []
    });

    const [meta, setMeta] = createStore<Meta>({
        columns: {},
    });

    createEffect(() => {
        // Dependencies
        routeValues.params.workspace, routeValues.params.schema, routeValues.params.table;

        setMeta({
            columns: {},
        });

        setData({
            columns: [],
            records: [],
        });
    })

    const [resource] = createResource((): FetchRecordsInput => {

        const columns = Object.values(meta.columns)
            .map(c => ({
                name: c.name,
                sort: c.sort,
                position: c.position,
                visible: unwrap(c).visible,
            }));

        const filters = Object.values(meta.columns)
            .map(c => c.filters
                .filter(f => f.upperValue)
                .map(f => ({
                    column: c.name,
                    discriminator: f.discriminator,
                    lowerValue: f.lowerValue,
                    upperValue: f.upperValue,
                }))
            )
            .reduce((agg, arr) => [...agg, ...arr], []);

        return debug({
            database: routeValues.params.workspace,
            schema: routeValues.params.schema,
            table: routeValues.params.table,
            columns: columns,
            filters: filters,
            // skip: params.skip,
            // take: params.take
        }, 'postFetchRecords input');

    }, async (input) => {

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

                const returnedColumns: Record<string, MetaColumn> = {};
                columns.forEach((c, i) => {
                    // Keep the reference if it exists
                    let metaColumn = meta.columns[c.name];

                    if (metaColumn) {
                        metaColumn.dataType = c.dataType;
                        metaColumn.nullable = c.nullable;
                    }
                    else {
                        metaColumn = {
                            name: c.name,
                            dataType: c.dataType,
                            nullable: c.nullable,
                            sort: Sort.Unsorted,
                            visible: true, // default to visible
                            position: i + 1, // default to initial position
                            filters: [],
                        };
                    }

                    returnedColumns[c.name] = metaColumn;
                });
                // Set so that missing columns are removed
                setMeta('columns', returnedColumns)
            }

            const records = resource()?.data?.records;
            if (records) {
                // TODO: Diff based on primary key or unique columns if known?
                setData('records', []);
                setData('records', records);
            }
        });
    });

    createEffect(() => titleSetter(routeValues.params.table ?? 'Table'));

    return (
        <section class='pr-1'>

            <Show when={resource.loading}>
                <Portal>
                    Loading…
                </Portal>
            </Show>
            <Show when={message()}>
                <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
                    {message()}
                </p>
            </Show>

            <Show when={activeRecord()}>
                {JSON.stringify(activeRecord())}
            </Show>

            <Show when={data.columns.length > 0}>

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

            </Show>

        </section>
    );
}
