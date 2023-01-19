import { Switch, Match, Show, createSignal } from "solid-js";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { OpenInodeDialog } from "../widgets/OpenInodeDialog";
import { AppInfo } from "./AppInfo";
import { AppProps } from "./AppProps";

export function TableApp(props: AppProps) {
    const [showOpenDialog, setShowOpenDialog] = createSignal(false);

    return <>
        <div class='pxy'>
            <Switch fallback={<>
                <div class='grid grid-cols-2 gap-1 w-300px'>
                    <button type='button' class='card text-center b b-solid'
                        onclick={() => setShowOpenDialog(true)}>
                        Open an Existing Table
                    </button>
                    <button type='button' class='card text-center b b-solid'
                        onclick={(e) => { }}>
                        Create a New Table
                    </button>
                </div>
            </>}>
                <Match when={TableAppInfo.canHandle(props.inode)}>
                    TODO: Table view/editor
                </Match>
            </Switch>
        </div>

        <Show when={showOpenDialog()}>
            <OpenInodeDialog
                type={InodeTypeId.Table}
                explorer={props.explorer}
                initialInode={props.inode}
                onDismiss={() => setShowOpenDialog(false)} />
        </Show>
    </>
}

export const TableAppInfo: AppInfo = {
    name: 'Table',
    component: TableApp,
    canHandle: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Table,
    showInAppDrawer: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Folder || inode.inodeTypeId === InodeTypeId.Space,
}

export default TableAppInfo;