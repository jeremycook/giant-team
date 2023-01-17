import { Accessor, Setter, createSignal, For, Show } from "solid-js";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { OrganizationDetails } from "../OrganizationDetailsResource"
import { ProcessOperator } from "../ProcessOperator"

export class InodeNavigator {
    private _root: Accessor<Inode>;
    private _setRoot: Setter<Inode>;

    constructor(rootInode: Inode) {
        [this._root, this._setRoot] = createSignal(rootInode);
    }

    get root() {
        return this._root();
    }

    refresh() {
        
    }
}

export function InodeElement(props: { inode: Inode }) {
    return <>
        {props.inode.name}
        <ul>
            <For each={props.inode.children}>{inode =>
                <li>
                    {inode.name}
                    <For each={inode.children}>{childNode =>
                        <InodeElement inode={childNode} />
                    }</For>
                </li>
            }</For>
        </ul>
    </>
}

export function InodeTree(props: {
    organization: OrganizationDetails,
    processOperator: ProcessOperator,
    navigator: InodeNavigator
}) {
    // TODO: Create InodeNavigator context
    return <>
        <InodeElement inode={props.navigator.root} />
    </>
}