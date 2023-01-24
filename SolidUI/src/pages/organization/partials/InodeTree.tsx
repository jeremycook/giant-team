import { Accessor, createSignal, For, Setter } from "solid-js";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { CaretRightIcon } from "../../../partials/Icons";
import { InodeProvider } from "./InodeProvider";


export function InodeTree(props: {
    inodeProvider: InodeProvider,
    selectedInode: Accessor<Inode | undefined>,
    onClickInode: (e: Event, inode: Inode, options: { isExpanded: boolean, expand: Setter<boolean> }) => void,
    inode: Inode,
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
                <For each={props.inodeProvider.getDirectoryContents(props.inode.path)}>{childInode =>
                    <InodeTree {...props} inode={childInode} />
                }</For>
            </ul>
        </li>
    </>
}

export function InodeRoot(props: {
    inodeProvider: InodeProvider,
    selectedInode: Accessor<Inode | undefined>,
    onClickInode: (e: Event, inode: Inode, options: { isExpanded: boolean, expand: Setter<boolean> }) => void,
}) {
    return <>
        <ul class='list-none pl-2 m-0'>
            <For each={props.inodeProvider.getDirectoryContents('')}>{inode =>
                <InodeTree {...props} inode={inode} />
            }</For>
        </ul>
    </>
}