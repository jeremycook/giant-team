import { Accessor, createSignal, For, Show } from 'solid-js';
import { SetStoreFunction, Store } from 'solid-js/store';
import { Portal } from 'solid-js/web';
import { JSX } from 'solid-js/web/types/jsx';
import { Sort } from '../api/GiantTeam';
import { EyeIcon, EyeOffIcon, FilterAddIcon, FilterIcon, OffIcon, SortAscIcon, SortDescIcon } from '../utils/icons';
import { debug } from '../utils/logging';

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
    position?: number;
    sort: Sort;
    visible: boolean;
}

interface ColumnPopupProps {
    setMeta: SetStoreFunction<Meta>;
    column: MetaColumn;
}

function ColumnPopup(props: ColumnPopupProps) {
    const { setMeta } = props;

    const column = () => props.column;

    return (
        <div>
            <div class='grid grid-cols-2 gap-2 mb items-center'>
                <div class='text-center'>Visibility</div>
                <div>
                    <div class='flex-inline items-center border children:p-1'>
                        <button onclick={() => setMeta?.('columns', column().name, 'visible', true)} class={(column().visible ? 'paint-primary border shadow shadow-inset' : '')}>
                            <EyeIcon /> <span class='sr-only'>Visible</span>
                        </button>
                        <button onclick={() => setMeta?.('columns', column().name, 'visible', false)} class={(column().visible !== true ? 'paint-primary border shadow shadow-inset' : '')}>
                            <EyeOffIcon /> <span class='sr-only'>Hidden</span>
                        </button>
                    </div>
                </div>
                <div class='text-center'>Sorting</div>
                <div>
                    <div class='flex-inline items-center border children:p-1 children:not-first:ml-1'>
                        <button onclick={() => setMeta?.('columns', column().name, 'sort', Sort.Asc)} class={(column().sort === Sort.Asc ? 'paint-primary border shadow shadow-inset' : '')}>
                            <SortAscIcon /> <span class='sr-only'>Sort Ascending</span>
                        </button>
                        <button onclick={() => setMeta?.('columns', column().name, 'sort', Sort.Desc)} class={(column().sort === Sort.Desc ? 'paint-primary border shadow shadow-inset' : '')}>
                            <SortDescIcon /> <span class='sr-only'>Sort Descending</span>
                        </button>
                        <button onclick={() => setMeta?.('columns', column().name, 'sort', Sort.Unsorted)} class={(column().sort === Sort.Unsorted ? 'paint-disabled border shadow shadow-inset' : '')}>
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
    setMeta?: SetStoreFunction<Meta>,
    headLeader?: () => JSX.Element,
    rowLeader?: (record: any[], rowIndex: Accessor<number>) => JSX.Element,
}) {

    const columns = () => [
        ...data.columns.map(name => meta.columns[name])
    ].sort((a, b) => ((a.position ?? 10000) - (b.position ?? 10000) || a.name.localeCompare(b.name)));

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
            left: (offsetX?: number) => (get().x + debug(offsetX ?? 0)) + 'px',
            top: (offsetY?: number) => (get().y + (offsetY ?? 0)) + 'px',
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

            <table>
                <thead>
                    <tr>
                        {headLeader?.()}
                        <For each={columns()}>{(column) =>
                            <Show when={column.visible}>
                                <th class='cursor-pointer' onclick={setMeta ? (e) => { setLastClick(e); toggleActiveColumn(column.name); } : undefined}>
                                    {column.name}
                                    {column.sort > 0 ? <FilterIcon /> : null}
                                </th>
                            </Show>
                        }</For>
                    </tr>
                </thead>
                <tbody>
                    <For each={data.records}>{(record, rowIndex) =>
                        <tr>
                            {rowLeader?.(record, rowIndex)}
                            <For each={columns()}>{(column, columnIndex) =>
                                <Show when={column.visible}>
                                    <td class='p-0'><div class='p-1 overflow-hidden max-w-200px whitespace-nowrap overflow-ellipsis'>{record[columnIndex()]}</div></td>
                                </Show>
                            }</For>
                        </tr>
                    }</For>
                </tbody>
            </table>

            <Show when={setMeta && activeColumn()}>

                <Portal mount={document.getElementById('main-portal')!}>
                    <div ref={setColumnPopupRef} class='card p-2 absolute max-w-100%' style={{ top: lastClick.top(20), left: lastClick.left(-(columnPopupRef()?.offsetWidth ?? 0) / 2) }}>
                        <div class='flex'>
                            <strong class='text-xl grow'>{activeColumn()!.name}</strong>
                            <button onclick={() => toggleActiveColumn(undefined)}>X</button>
                        </div>
                        <hr />
                        <ColumnPopup
                            setMeta={setMeta!}
                            column={activeColumn()!}
                        />
                    </div>
                </Portal>

            </Show>

        </div>
    );
}