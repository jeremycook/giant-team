import { createStore, unwrap } from 'solid-js/store';
import { batch, createEffect, createResource, Show } from 'solid-js';
import { FetchRecordsInput, Sort } from '../../../api/GiantTeam';
import { postFetchRecords } from '../../../api/GiantTeam.Data.Api';
import { setTitle } from '../../../utils/page';
import { Data, DataRecord, Meta, MetaColumn } from '../../../widgets/SmartTable';
import SmartTable from '../../../widgets/SmartTable';
import { Portal } from 'solid-js/web';
import { useParams } from '@solidjs/router';

export default function WorkspacePage() {
    setTitle('Table');

    const params = useParams();

    const [data, setData] = createStore<Data>({
        columns: [],
        records: []
    });

    const [meta, setMeta] = createStore<Meta>({
        columns: {},
    });

    createEffect(() => {
        // Dependencies
        params.workspace, params.zone, params.table;

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

        return {
            database: params.workspace,
            schema: params.zone,
            table: params.table,
            columns: columns,
            filters: filters,
            // skip: params.skip,
            // take: params.take
        };

    }, async (input) => {

        const output = await postFetchRecords(input)
        return output;

    });

    const ok = () => resource()?.ok == true;
    const message = () => resource()?.message || null;
    // const [selectedRecordIndex, setSelectedRecordIndex] = createSignal<number>();
    // const activeColumnRecord = () => typeof selectedRecordIndex() === 'number' ?
    //     [...data.columns.map((name, i) => ({
    //         column: meta.columns[name],
    //         value: data.records[selectedRecordIndex()!][i],
    //     }))]
    //         .filter(c => c.column.visible)
    //         .sort((a, b) => a.column.position - b.column.position || a.column.name.localeCompare(b.column.name)) :
    //     undefined;
    // const recordDialogInfo = createMutable({ x: 0, y: 0, indexes: [] as number[] });

    // const toggleActiveRecord = (recordIndex?: number, e?: MouseEvent) => {
    //     if (typeof recordIndex === 'number' && recordIndex !== selectedRecordIndex()) {
    //         if (e) {
    //             const target = e.currentTarget as HTMLButtonElement;
    //             const position = getElementPosition(target);
    //             recordDialogInfo.x = position.x + target.clientWidth;
    //             recordDialogInfo.y = position.y + target.clientHeight;
    //         }
    //         setSelectedRecordIndex(recordIndex);
    //     }
    //     else {
    //         setSelectedRecordIndex(undefined);
    //     }
    // };

    createEffect(() => {
        batch(() => {

            const response = resource();
            const data = response?.ok ? response.data : undefined;

            const columns = data?.columns;
            if (columns) {
                setData('columns', columns.map(c => c.name));

                const returnedColumns: Record<string, MetaColumn> = {};
                columns.forEach((c, i) => {
                    let metaColumn = unwrap(meta.columns[c.name]);

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

            const records = data?.records;
            if (records) {
                // TODO: Diff based on primary key or unique columns if known?
                setData('records', []);
                const dataRecords = records
                    .map(r => ({
                        selected: false,
                        record: r!,
                    }) as DataRecord);
                setData('records', dataRecords);
            }
        });
    });

    // createEffect(() => setTitle(params.table ?? 'Table'));

    // REMOVE
    // setTimeout(() => data.records.length > 0 ? toggleActiveRecord(1) : null, 500);

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

            <Show when={data.columns.length > 0}>

                <SmartTable
                    data={data}
                    meta={meta}
                    setMeta={setMeta}
                    onClickRecordSelector={(e, i) => setData('records', i, 'selected', s => !s)}
                // headLeader={() => <th></th>}
                // rowLeader={(_, recordIndex) => <td class='h-1px p-0 b-0'>
                //     <button type='button' class='flex justify-end items-center w-100% h-100% p-1px border paint-primary ' onclick={e => {
                //         recordDialogInfo.x = e.pageX + 20;
                //         recordDialogInfo.y = e.pageY;
                //         toggleActiveRecord(recordIndex(), e);
                //     }}>
                //         {recordIndex() + 1}
                //     </button>
                // </td>}
                ></SmartTable>

            </Show>

            {/* <Show when={typeof selectedRecordIndex() === 'number'}>

                <Dialog
                    title={'Active Record'}
                    onDismiss={() => toggleActiveRecord()}
                    initialPosition={recordDialogInfo}>
                    <form class='form-grid' onSubmit={e => e.preventDefault()}>
                        <ColumnRecordFields fields={activeColumnRecord()!} />

                        <div />
                        <div>
                            <button class='button'>
                                Save Changes
                            </button>
                        </div>
                    </form>
                </Dialog>

            </Show> */}


        </section >
    );
}
