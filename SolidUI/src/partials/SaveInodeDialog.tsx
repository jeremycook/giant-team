import { createSignal, JSX } from "solid-js";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models";
import { InodeExplorer } from "../pages/organization/partials/InodeExplorerContext";
import { InodeRoot } from "../pages/organization/partials/InodeTree";
import Dialog from "../widgets/Dialog";

export function SaveInodeDialog(props: {
    type: InodeTypeId,
    explorer: InodeExplorer,
    initialInode: Inode,
    onDismiss: JSX.EventHandlerUnion<HTMLButtonElement, MouseEvent>,
    onSubmit: (e: SubmitEvent & { currentTarget: HTMLFormElement }, parentInode: Inode) => void
}) {
    const [parentInode, setParentInode] = createSignal(props.initialInode);

    return <>
        <Dialog title='Save As' onDismiss={props.onDismiss}>
            <form onsubmit={e => props.onSubmit(e, parentInode())}>
                <div class='flex flex-col gap-1'>
                    <div class='h-200px b b-solid pxy'>
                        <InodeRoot
                            inode={props.explorer.root}
                            selectedInode={parentInode}
                            onClickInode={(e, inode, { isExpanded, expand }) => {
                                setParentInode(inode);
                                expand(true);
                            }} />
                    </div>
                    <input placeholder={props.type + ' Name'} required />
                    <div class='flex gap-1 justify-end'  >
                        <button class='button-primary'>Save</button>
                        <button type='button' class='button'>Cancel</button>
                    </div>
                </div>
            </form>
        </Dialog>
    </>
}