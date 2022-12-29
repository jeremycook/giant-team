import { For } from 'solid-js';
import { produce, SetStoreFunction, Store } from 'solid-js/store';
import { FetchRecordsInputRangeFilter, Sort } from '../api/GiantTeam';
import { EyeIcon, EyeOffIcon, FilterAddIcon, LeftIcon, OffIcon, RightIcon, SortAscIcon, SortDescIcon } from '../helpers/icons';
import { Meta, MetaColumn } from './SmartTable';

export interface ColumnDialogProps {
    meta: Store<Meta>;
    setMeta: SetStoreFunction<Meta>;
    column: MetaColumn;
}

export function ColumnDialog(props: ColumnDialogProps) {
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

            setMeta('columns', column().name, 'position', newPos);
        }
        else if (newPos > pos) {
            // Moving right (old------->new)
            Object.entries(meta.columns)
                .filter(pair => pos < pair[1].position && pair[1].position <= newPos)
                .forEach(pair => setMeta('columns', pair[0], 'position', pos));

            setMeta('columns', column().name, 'position', newPos);
        }
    };

    return (
        <div>
            <div class='grid grid-cols-2 gap-2 mb items-center'>
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
                <div class='text-center'>Position</div>
                <div>
                    <div class='flex-inline items-center border p-1px children:p-1'>
                        <button type='button' onclick={() => moveTo(column().position - 1)} class='border'>
                            <LeftIcon /> <span class='sr-only'>Move Left</span>
                        </button>
                        <input value={column().position} onchange={e => moveTo(parseInt((e.target as HTMLInputElement).value) || 0)} type='number' min='1' max={Object.keys(meta.columns).length} class='w-3em text-center border-0' />
                        <button type='button' onclick={() => moveTo(column().position + 1)} class='border'>
                            <RightIcon /> <span class='sr-only'>Move Right</span>
                        </button>
                    </div>
                </div>
                <div class='text-center'>Filters</div>
                <div>
                    <button type='button' class='button p-1'
                        onclick={() => setMeta('columns', column().name, 'filters',
                            f => [...f, {
                                discriminator: 'FetchRecordsInputRangeFilter',
                                column: column().name,
                                lowerValue: '',
                                upperValue: '',
                            } as FetchRecordsInputRangeFilter])}>
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
