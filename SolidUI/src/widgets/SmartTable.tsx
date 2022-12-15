import { Accessor, createSignal, For, Show } from 'solid-js';
import { SetStoreFunction, Store, unwrap } from 'solid-js/store';
import { Portal } from 'solid-js/web';
import { JSX } from 'solid-js/web/types/jsx';
import { Sort } from '../api/GiantTeam';
import { DismissIcon, EyeIcon, EyeOffIcon, FilterAddIcon, LeftIcon, OffIcon, RightIcon, SortAscIcon, SortDescIcon } from '../utils/icons';

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
                        <button onclick={() => moveTo(column().position - 1)} class='border'>
                            <LeftIcon /> <span class='sr-only'>Move Left</span>
                        </button>
                        <input value={column().position} onchange={e => moveTo(parseInt((e.target as HTMLInputElement).value))} type='number' min='1' max={Object.keys(meta.columns).length} class='w-3em text-center border-0' />
                        <button onclick={() => moveTo(column().position + 1)} class='border'>
                            <RightIcon /> <span class='sr-only'>Move Right</span>
                        </button>
                    </div>
                </div>
                <div class='text-center'>Visibility</div>
                <div>
                    <div class='flex-inline items-center border children:p-1'>
                        <button onclick={() => setMeta('columns', column().name, 'visible', true)} class={(column().visible ? 'paint-primary border shadow shadow-inset' : '')}>
                            <EyeIcon /> <span class='sr-only'>Visible Column</span>
                        </button>
                        <button onclick={() => setMeta('columns', column().name, 'visible', false)} class={(!column().visible ? 'paint-primary border shadow shadow-inset' : '')}>
                            <EyeOffIcon /> <span class='sr-only'>Hidden Column</span>
                        </button>
                    </div>
                </div>
                <div class='text-center'>Sorting</div>
                <div>
                    <div class='flex-inline items-center border children:p-1 children:not-first:ml-1'>
                        <button onclick={() => setMeta('columns', column().name, 'sort', Sort.Asc)} class={(column().sort === Sort.Asc ? 'paint-primary border shadow shadow-inset' : '')}>
                            <SortAscIcon /> <span class='sr-only'>Sort Ascending</span>
                        </button>
                        <button onclick={() => setMeta('columns', column().name, 'sort', Sort.Desc)} class={(column().sort === Sort.Desc ? 'paint-primary border shadow shadow-inset' : '')}>
                            <SortDescIcon /> <span class='sr-only'>Sort Descending</span>
                        </button>
                        <button onclick={() => setMeta('columns', column().name, 'sort', Sort.Unsorted)} class={(column().sort === Sort.Unsorted ? 'paint-disabled border shadow shadow-inset' : '')}>
                            <OffIcon /> <span class='sr-only'>Disable Sorting</span>
                        </button>
                    </div>
                </div>
            </div>
            <div>
                <button>
                    <FilterAddIcon /> Add a Filter
                </button>
            </div>
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

                    <div class='flex-inline items-center border p-1px children:p-1 children:border'>
                        Hidden Columns:&nbsp;
                        <For each={columns()}>{(column) =>
                            <Show when={!column.visible}>
                                <button onclick={(e) => { setLastClick(e); toggleActiveColumn(column.name); }} class='flex gap-1'>
                                    {column.name}
                                    {column.sort === Sort.Asc ? <><SortAscIcon /> <strong class='text-xs'>{column.position}</strong></> : null}
                                    {column.sort === Sort.Desc ? <SortDescIcon /> : null}
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
                                    <div class='relative'>
                                        <div class='absolute position-right'>
                                            {column.sort === Sort.Asc ? <SortAscIcon /> : null}
                                            {column.sort === Sort.Desc ? <SortDescIcon /> : null}
                                        </div>
                                    </div>
                                    <button onclick={(e) => { setLastClick(e); toggleActiveColumn(column.name); }}
                                        class='block w-100% py-1'>
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
                            <button onclick={() => toggleActiveColumn(undefined)}>
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