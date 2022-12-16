import { Accessor, createSignal, For, Show } from 'solid-js';
import { createMutable, SetStoreFunction, Store } from 'solid-js/store';
import { JSX } from 'solid-js/web/types/jsx';
import { FetchRecordsInputRangeFilter, Sort } from '../api/GiantTeam';
import { FilterIcon, SortAscIcon, SortDescIcon } from '../utils/icons';
import { ColumnDialog } from './ColumnDialog';
import Dialog from './Dialog';

export interface Data {
    columns: string[];
    records: any[][];
}

export interface Meta {
    columns: Record<string, MetaColumn>;
}

export interface MetaColumn {
    name: string;
    dataType: string;
    nullable: boolean;
    position: number;
    sort: Sort;
    visible: boolean;
    filters: MetaFilter[];
}

export interface MetaFilter extends FetchRecordsInputRangeFilter {
}

export default function Table({
    data,
    meta,
    setMeta,
    headLeader,
    rowLeader,
}: {
    data: Store<Data> | Data,
    meta: Store<Meta> | Meta,
    setMeta: SetStoreFunction<Meta>,
    headLeader?: () => JSX.Element,
    rowLeader?: (record: any[], rowIndex: Accessor<number>) => JSX.Element,
}) {

    const columns = () => [
        ...Object.values(meta.columns).map((c) => ({
            ...c,
            columnIndex: data.columns.indexOf(c.name)
        }))
    ].sort((a, b) => (a.position - b.position || a.name.localeCompare(b.name)));

    const hiddenColumns = () => columns().filter(c => !c.visible);

    const { activeColumn, toggleActiveColumn } = (() => {
        const [activeColumnName, set] = createSignal<string>();
        const toggleActiveColumnName = (columnName?: string) => {
            if (activeColumnName() !== columnName) {
                set(columnName);
            }
            else {
                set(undefined);
            }
        }
        const activeColumn = () => activeColumnName() ? meta.columns[activeColumnName()!] : null;
        return { activeColumn, toggleActiveColumn: toggleActiveColumnName };
    })();

    const columnDialogPosition = createMutable({ left: 0, top: 0 });

    return (
        <div>
            <div class='min-h-3rem'>
                <Show when={hiddenColumns().length > 0}>

                    <div class='flex-inline gap-1px border p-1px children:p-1'>
                        <div>
                            Hidden Columns
                        </div>
                        <For each={columns()}>{(column) =>
                            <Show when={!column.visible}>
                                <button type='button' class='border'
                                    onclick={(e) => { columnDialogPosition.left = e.pageX; columnDialogPosition.top = e.pageY; toggleActiveColumn(column.name); }}>
                                    {column.name}
                                    {column.sort === Sort.Asc ? <SortAscIcon /> : null}
                                    {column.sort === Sort.Desc ? <SortDescIcon /> : null}
                                    {column.filters.length ? <FilterIcon /> : null}
                                </button>
                            </Show>
                        }</For>
                    </div>
                </Show>
            </div>

            <table>
                <thead>
                    <tr>
                        {headLeader?.()}
                        <For each={columns()}>{(column) =>
                            <Show when={column.visible}>
                                <th class='p-0'>
                                    <button type='button' class='block w-100% p-1 border paint-primary'
                                        onclick={(e) => { columnDialogPosition.left = e.pageX; columnDialogPosition.top = e.pageY; toggleActiveColumn(column.name); }}>
                                        <div>
                                            {column.sort === Sort.Asc ? <SortAscIcon /> : null}
                                            {column.sort === Sort.Desc ? <SortDescIcon /> : null}
                                            {column.filters.length ? <FilterIcon /> : null}
                                        </div>
                                        {column.name}
                                    </button>
                                </th>
                            </Show>
                        }</For>
                    </tr>
                </thead>
                <tbody>
                    <For each={data.records}>{(record, rowIndex) =>
                        <tr>
                            {rowLeader?.(record, rowIndex)}
                            <For each={columns()}>{(column) =>
                                <Show when={column.visible}>
                                    <td class='p-0'><div class='p-1 overflow-hidden max-w-200px whitespace-nowrap overflow-ellipsis'>{record[column.columnIndex]}</div></td>
                                </Show>
                            }</For>
                        </tr>
                    }</For>
                </tbody>
            </table>

            <Show when={activeColumn()}>

                <Dialog
                    title={activeColumn()!.name}
                    onDismiss={() => toggleActiveColumn(undefined)}
                    initialPosition={() => ({ left: columnDialogPosition.left, top: columnDialogPosition.top + 20 })}>
                    <ColumnDialog
                        meta={meta}
                        setMeta={setMeta}
                        column={activeColumn()!}
                    />
                </Dialog>

            </Show>

        </div>
    );
}