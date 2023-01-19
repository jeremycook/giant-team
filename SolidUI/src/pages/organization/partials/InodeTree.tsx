import { Accessor, createSignal, For, Match, Show, Switch } from "solid-js";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { CaretDownIcon, CaretRightIcon } from "../../../partials/Icons";


export function InodeTree(props: {
    inode: Inode,
    selectedInode: Accessor<Inode | undefined>,
    onClickInode: (inode: Inode) => void,
}) {
    const [isExpanded, expand] = createSignal(false);

    return <>
        <li>
            <button
                onclick={() => expand(!isExpanded())}>
                <CaretRightIcon class='transition-transform transition-duration-100' classList={{
                    'rotate-90': isExpanded(),
                }} />
            </button>
            {' '}
            <button type='button'
                classList={{
                    'paint-primary': props.inode === props.selectedInode()
                }}
                onclick={() => props.onClickInode(props.inode)}>
                {props.inode.name}
            </button>

            <ul class='list-none pl-2 m-0 transition-all'
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
    inode: Inode,
    selectedInode: Accessor<Inode | undefined>,
    onClickInode: (inode: Inode) => void,
}) {
    return <>
        <ul class='list-none pl-2 m-0'>
            <For each={props.inode.children}>{inode =>
                <InodeTree inode={inode} onClickInode={props.onClickInode} selectedInode={props.selectedInode} />
            }</For>
        </ul>
    </>
}