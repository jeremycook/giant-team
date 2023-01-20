import { Switch, Match, createSignal } from "solid-js";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { OpenInodeDialog } from "../partials/OpenInodeDialog";
import { SaveInodeDialog } from "../partials/SaveInodeDialog";
import { AppInfo } from "./AppInfo";
import { AppProps } from "./AppProps";

enum DialogType {
    None,
    Open,
    Save,
}

export function TableApp(props: AppProps) {
    const [dialogType, setDialogType] = createSignal(DialogType.None);

    return <>
        <div class='pxy'>
            <Switch fallback={<>
                <div class='grid grid-cols-2 gap-1 w-300px'>
                    <button type='button' class='card text-center b b-solid'
                        onclick={() => setDialogType(DialogType.Open)}>
                        Open an Existing Table
                    </button>
                    <button type='button' class='card text-center b b-solid'
                        onclick={() => setDialogType(DialogType.Save)}>
                        Create a New Table
                    </button>
                </div>
            </>}>
                <Match when={TableAppInfo.canHandle(props.inode)}>
                    TODO: Table view/editor
                </Match>
            </Switch>
        </div>

        <Switch>
            <Match when={dialogType() === DialogType.Open}>
                <OpenInodeDialog
                    type={InodeTypeId.Table}
                    explorer={props.explorer}
                    initialInode={props.inode}
                    onDismiss={() => setDialogType(DialogType.None)} />
            </Match>
            <Match when={dialogType() === DialogType.Save}>
                <SaveInodeDialog
                    type={InodeTypeId.Table}
                    explorer={props.explorer}
                    initialInode={props.inode}
                    onDismiss={() => setDialogType(DialogType.None)}
                    onSubmit={(e, parentInode) => {
                        e.preventDefault();
                        // Save table
                        // TODO: postCreateT
                    }} />
            </Match>
        </Switch>
    </>
}

export const TableAppInfo: AppInfo = {
    name: 'Table',
    component: TableApp,
    canHandle: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Table,
    showInAppDrawer: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Folder || inode.inodeTypeId === InodeTypeId.Space,
}

export default TableAppInfo;