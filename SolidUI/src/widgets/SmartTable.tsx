import { Accessor, createSignal, For, Show } from 'solid-js';
import { createMutable, SetStoreFunction, Store } from 'solid-js/store';
import { JSX } from 'solid-js/web/types/jsx';
import { Sort, FetchRecordsInputRangeFilter } from '../bindings/GiantTeam.Organization.Services';
import { getElementPosition } from '../helpers/htmlHelpers';
import { FilterIcon, SortAscIcon, SortDescIcon } from '../helpers/icons';
import { ColumnDialog } from './ColumnDialog';
import Dialog, { DialogAnchor } from './Dialog';

export interface Data {
    columns: string[];
    records: DataRecord[];
}

export interface DataRecord {
    selected: boolean;
    record: any[];
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
    onClickRecordSelector: onClickRow,
}: {
    data: Store<Data> | Data,
    meta: Store<Meta> | Meta,
    setMeta: SetStoreFunction<Meta>,
    headLeader?: () => JSX.Element,
    rowLeader?: (recordIndex: Accessor<number>) => JSX.Element,
    onClickRecordSelector?: (event: Event, recordIndex: number) => void,
}) {

    const columns = () => [
        ...Object.values(meta.columns).map((c) => ({
            ...c,
            columnIndex: data.columns.indexOf(c.name)
        }))
    ].sort((a, b) => (a.position - b.position || a.name.localeCompare(b.name)));

    const hiddenColumns = () => columns().filter(c => !c.visible);

    const columnDialogPosition = createMutable({ x: 0, y: 0 });

    const { activeColumn, toggleActiveColumn: onClickColumn } = (() => {
        const [activeColumnName, set] = createSignal<string>();
        const toggleActiveColumn = (columnName?: string, e?: MouseEvent) => {
            if (columnName && activeColumnName() !== columnName) {

                if (e) {
                    columnDialogPosition.x = e.pageX;
                    const target = e.currentTarget as HTMLElement;
                    columnDialogPosition.y = getElementPosition(target).top + target.clientHeight;
                }

                set(columnName);
            }
            else {
                set(undefined);
            }
        }
        const activeColumn = () => activeColumnName() ? meta.columns[activeColumnName()!] : null;
        return { activeColumn, toggleActiveColumn };
    })();


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
                                    onclick={e => onClickColumn(column.name, e)}>
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
                        {typeof onClickRow === 'function' && <th></th>}
                        <For each={columns()}>{(column) =>
                            <Show when={column.visible}>
                                <th role='button' class='b'
                                    onclick={(e) => onClickColumn(column.name, e)}>
                                    <div>
                                        {column.sort === Sort.Asc ? <SortAscIcon /> : null}
                                        {column.sort === Sort.Desc ? <SortDescIcon /> : null}
                                        {column.filters.length ? <FilterIcon /> : null}
                                    </div>
                                    <strong>{column.name}</strong>
                                </th>
                            </Show>
                        }</For>
                    </tr>
                </thead>
                <tbody>
                    <For each={data.records}>{(record, recordIndex) =>
                        <tr>
                            {rowLeader?.(recordIndex)}
                            {typeof onClickRow === 'function' &&
                                <td role='button' class={'b ' + (record.selected ? 'paint-primary' : '')}
                                    onclick={e => onClickRow(e, recordIndex())}>
                                    {recordIndex() + 1}
                                </td>
                            }
                            <For each={columns()}>{(column) =>
                                <Show when={column.visible}>
                                    <td class='p-0'><div class='p-1 overflow-hidden max-w-200px whitespace-nowrap overflow-ellipsis'>{record.record[column.columnIndex]}</div></td>
                                </Show>
                            }</For>
                        </tr>
                    }</For>
                </tbody>
            </table>

            <Show when={activeColumn()}>

                <Dialog
                    title={activeColumn()!.name}
                    onDismiss={() => onClickColumn()}
                    initialPosition={columnDialogPosition}
                    anchor={DialogAnchor.topCenter}>
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