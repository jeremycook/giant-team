import { JSX } from "solid-js";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models";
import { InodeExplorer } from "../pages/organization/partials/InodeExplorerContext";
import Dialog from "../widgets/Dialog";

export function OpenInodeDialog(props: {
    type: InodeTypeId,
    explorer: InodeExplorer,
    initialInode: Inode,
    onDismiss: JSX.EventHandlerUnion<HTMLButtonElement, MouseEvent>,
}) {
    return <>
        <Dialog title='Open'
            onDismiss={props.onDismiss}>
            <div>TODO</div>
        </Dialog>
    </>
}