import { JSX } from "solid-js";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models";
import { InodeProvider } from "../pages/organization/partials/InodeProvider";
import Dialog from "../widgets/Dialog";

export function OpenInodeDialog(props: {
    type: InodeTypeId,
    inodeProvider: InodeProvider,
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