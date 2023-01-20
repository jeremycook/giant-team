import { Accessor, createSignal, For, Setter } from "solid-js";
import { CaretRightIcon } from "../../../partials/Icons";
import { ExplorerInode } from "./InodeExplorerContext";


export function InodeTree(props: {
    inode: ExplorerInode,
    selectedInode: Accessor<ExplorerInode | undefined>,
    onClickInode: (e: Event, inode: ExplorerInode, options: { isExpanded: boolean, expand: Setter<boolean> }) => void,
}) {
    const [isExpanded, expand] = createSignal(false);

    const toggleExpanded = () => expand(!isExpanded());

    return <>
        <li>
            <button type='button'
                onclick={() => toggleExpanded()}>
                <CaretRightIcon class='transition-transform transition-duration-100' classList={{
                    'rotate-90': isExpanded(),
                }} />
            </button>
            {' '}
            <button type='button'
                classList={{
                    'paint-primary': props.inode === props.selectedInode()
                }}
                onclick={e => props.onClickInode(e, props.inode, { isExpanded: isExpanded(), expand })}>
                {props.inode.name}
            </button>

            <ul class='list-none pl-2 m-0 overflow-hidden transition-all transition-duration-100'
                classList={{
                    'h-0': !isExpanded(),
                }}>
                <For each={props.inode.children}>{childNode =>
                    <InodeTree inode={childNode} selectedInode={props.selectedInode} onClickInode={props.onClickInode} />
                }</For>
            </ul>
        </li>
    </>
}

export function InodeRoot(props: {
    inode: ExplorerInode,
    selectedInode: Accessor<ExplorerInode | undefined>,
    onClickInode: (e: Event, inode: ExplorerInode, options: { isExpanded: boolean, expand: Setter<boolean> }) => void,
}) {
    return <>
        <ul class='list-none pl-2 m-0'>
            <For each={props.inode.children}>{inode =>
                <InodeTree {...props} />
            }</For>
        </ul>
    </>
}