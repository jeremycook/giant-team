import { Accessor, createSignal, For, Show } from 'solid-js';
import { produce, SetStoreFunction, Store } from 'solid-js/store';
import { Portal } from 'solid-js/web';
import { JSX } from 'solid-js/web/types/jsx';
import { FetchRecordsInputRangeFilter, Sort } from '../api/GiantTeam';
import { DismissIcon, EyeIcon, EyeOffIcon, FilterAddIcon, FilterIcon, LeftIcon, OffIcon, RightIcon, SortAscIcon, SortDescIcon } from '../utils/icons';

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

interface ColumnPopupProps {
    meta: Store<Meta>;
    setMeta: SetStoreFunction<Meta>;
    column: MetaColumn;
}

function ColumnPopup(props: ColumnPopupProps) {
    const { meta, setMeta } = props;

    const column = () => props.column;

    // Range from 1..length
    const moveTo = (newPos: number) => {
        const pos = column().position;
        newPos = Math.max(1, Math.min(Object.keys(meta.columns).length, newPos));
        if (newPos < pos) {
            // Moving left (new<------old)
            Object.entries(meta.columns)
                .filter(pair => newPos <= pair[1].position && pair[1].position < pos)
                .forEach(pair => setMeta('columns', pair[0], 'position', pos));

            setMeta('columns', column().name, 'position', newPos)
        }
        else if (newPos > pos) {
            // Moving right (old------->new)
            Object.entries(meta.columns)
                .filter(pair => pos < pair[1].position && pair[1].position <= newPos)
                .forEach(pair => setMeta('columns', pair[0], 'position', pos));

            setMeta('columns', column().name, 'position', newPos)
        }
    };

    return (
        <div>
            <div class='grid grid-cols-2 gap-2 mb items-center'>
                <div class='text-center'>Position</div>
                <div>
                    <div class='flex-inline items-center border p-1px children:p-1'>
                        <button type='button' onclick={() => moveTo(column().position - 1)} class='border'>
                            <LeftIcon /> <span class='sr-only'>Move Left</span>
                        </button>
                        <input value={column().position} onchange={e => moveTo(parseInt((e.target as HTMLInputElement).value))} type='number' min='1' max={Object.keys(meta.columns).length} class='w-3em text-center border-0' />
                        <button type='button' onclick={() => moveTo(column().position + 1)} class='border'>
                            <RightIcon /> <span class='sr-only'>Move Right</span>
                        </button>
                    </div>
                </div>
                <div class='text-center'>Visibility</div>
                <div>
                    <div class='flex-inline items-center border children:p-1'>
                        <button type='button' onclick={() => setMeta('columns', column().name, 'visible', true)} class={(column().visible ? 'paint-primary border shadow shadow-inset' : '')}>
                            <EyeIcon /> <span class='sr-only'>Visible Column</span>
                        </button>
                        <button type='button' onclick={() => setMeta('columns', column().name, 'visible', false)} class={(!column().visible ? 'paint-primary border shadow shadow-inset' : '')}>
                            <EyeOffIcon /> <span class='sr-only'>Hidden Column</span>
                        </button>
                    </div>
                </div>
                <div class='text-center'>Sorting</div>
                <div>
                    <div class='flex-inline items-center border children:p-1 children:not-first:ml-1'>
                        <button type='button' onclick={() => setMeta('columns', column().name, 'sort', Sort.Asc)} class={(column().sort === Sort.Asc ? 'paint-primary border shadow shadow-inset' : '')}>
                            <SortAscIcon /> <span class='sr-only'>Sort Ascending</span>
                        </button>
                        <button type='button' onclick={() => setMeta('columns', column().name, 'sort', Sort.Desc)} class={(column().sort === Sort.Desc ? 'paint-primary border shadow shadow-inset' : '')}>
                            <SortDescIcon /> <span class='sr-only'>Sort Descending</span>
                        </button>
                        <button type='button' onclick={() => setMeta('columns', column().name, 'sort', Sort.Unsorted)} class={(column().sort === Sort.Unsorted ? 'paint-disabled border shadow shadow-inset' : '')}>
                            <OffIcon /> <span class='sr-only'>Disable Sorting</span>
                        </button>
                    </div>
                </div>
                <div class='text-center'>Filters</div>
                <div>
                    <button type='button' class='button p-1'
                        onclick={() => setMeta('columns', column().name, 'filters',
                            f => [...f, {
                                column: column().name,
                                discriminator: 'FetchRecordsInputRangeFilter',
                                lowerValue: '',
                                upperValue: '',
                            }])}>
                        <FilterAddIcon /> Add a Filter
                    </button>
                </div>
            </div>
            <For each={column().filters}>{(filter, i) => (
                <div class='flex p-1 border children:p-1'>
                    <input class='w-10rem'
                        value={filter.lowerValue} onchange={(e: any) => setMeta('columns', produce(pair => pair[column().name].filters[i()].lowerValue = e.target.value ?? ''))} />
                    <input class='w-10rem'
                        value={filter.upperValue} onchange={(e: any) => setMeta('columns', produce(pair => pair[column().name].filters[i()].upperValue = e.target.value ?? ''))} />
                    <button type='button' onclick={() => setMeta('columns', column().name, 'filters', [])}>Remove</button>
                </div>
            )}</For>
        </div>
    );
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

    const [columnPopupRef, setColumnPopupRef] = createSignal<HTMLElement>();
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
    const { lastClick, setLastClick } = (() => {
        const [get, set] = createSignal({ x: 0, y: 0 });
        const lastClick = {
            x: () => get().x,
            y: () => get().y,
            left: (el?: HTMLElement) => Math.min(document.body.clientWidth - (el?.clientWidth ?? 0), Math.max(0, get().x - ((el?.clientWidth ?? 0) / 2))) + 'px',
            top: (el?: HTMLElement) => Math.min(document.body.clientHeight - (el?.clientHeight ?? 0), Math.max(0, get().y + 20)) + 'px',
        };
        const setLastClick = (e: MouseEvent) => {
            set({
                x: e.clientX,
                y: e.clientY,
            })
        };
        return { lastClick, setLastClick };
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
                                    onclick={(e) => { setLastClick(e); toggleActiveColumn(column.name); }}>
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
                                        onclick={(e) => { setLastClick(e); toggleActiveColumn(column.name); }}>
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

                <Portal mount={document.getElementById('main-portal')!}>
                    <div ref={setColumnPopupRef} class='card p-2 absolute max-w-100%' style={{ top: lastClick.top(columnPopupRef()!), left: lastClick.left(columnPopupRef()!) }} role='dialog'>
                        <div class='flex mb'>
                            <strong class='text-xl grow'>{activeColumn()!.name}</strong>
                            <button type='button' onclick={() => toggleActiveColumn(undefined)}>
                                <DismissIcon />
                                <span class='sr-only'>Close Dialog</span>
                            </button>
                        </div>
                        <ColumnPopup
                            meta={meta}
                            setMeta={setMeta}
                            column={activeColumn()!}
                        />
                    </div>
                </Portal>

            </Show>

        </div>
    );
}