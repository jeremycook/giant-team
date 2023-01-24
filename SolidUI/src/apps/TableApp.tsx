import { Switch, Match, createSignal, JSX, For } from "solid-js";
import { postCreateTable } from "../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { TabularData } from "../bindings/GiantTeam.Postgres.Models";
import { createQueryResource } from "../pages/organization/resources/QueryResource";
import { AddIcon } from "../partials/Icons";
import { OpenInodeDialog } from "../partials/OpenInodeDialog";
import { SaveInodeDialog } from "../partials/SaveInodeDialog";
import { toast } from "../partials/Toasts";
import { ShowItem } from "../widgets/ShowItem";
import { AppInfo } from "./AppInfo";
import { AppProps } from "./AppProps";

enum DialogState {
    Closed,
    Open,
    Save,
}

export function TableApp(props: AppProps) {
    const [dialogState, setDialogState] = createSignal(DialogState.Closed);

    const schemaName = () => {
        return props.process.inode.path.split('/')[0];
    }
    const queryResource = createQueryResource({
        organization: props.organization.organizationId,
        sql: `
        SELECT *
        FROM "${schemaName()}"."${props.process.inode.uglyName}"
        `
    });

    return <>
        <div class='pxy'>
            <Switch fallback={<>
                <div class='grid grid-cols-2 gap-1 w-300px'>
                    <button type='button' class='card text-center b b-solid'
                        onclick={() => setDialogState(DialogState.Open)}>
                        Open an Existing Table
                    </button>
                    <button type='button' class='card text-center b b-solid'
                        onclick={() => setDialogState(DialogState.Save)}>
                        Create a New Table
                    </button>
                </div>
            </>}>
                <Match when={TableAppInfo.canHandle(props.process.inode)}>
                    <ShowItem when={queryResource.data}>{tabularData => <>
                        <Table data={tabularData} />
                    </>}</ShowItem>
                </Match>
            </Switch>
        </div>

        <Switch>
            <Match when={dialogState() === DialogState.Open}>
                <OpenInodeDialog
                    type={InodeTypeId.Table}
                    inodeProvider={props.inodeProvider}
                    initialInode={props.process.inode}
                    onDismiss={() => setDialogState(DialogState.Closed)} />
            </Match>
            <Match when={dialogState() === DialogState.Save}>
                <SaveInodeDialog
                    type={InodeTypeId.Table}
                    inodeProvider={props.inodeProvider}
                    initialInode={props.process.inode}
                    onDismiss={() => setDialogState(DialogState.Closed)}
                    onSubmit={async (e, m) => {
                        e.preventDefault();

                        // Save table
                        const response = await postCreateTable({
                            organizationId: props.inodeProvider.organization.organizationId,
                            parentInodeId: m.parentInode.inodeId,
                            tableName: m.name,
                            accessControlList: m.accessControls,
                        });

                        if (response.ok) {
                            toast.success('Table created.');
                            await props.inodeProvider.refresh(response.data.path);
                            props.process.setInode(response.data);
                            setDialogState(DialogState.Closed);
                        }
                        else {
                            toast.error(response.message);
                        }
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

function Table({ data, rowLeader }: { data: TabularData, rowLeader?: (record: any[]) => JSX.Element }) {
    return (
        <table>
            <thead>
                <tr>
                    {typeof rowLeader === 'function' && <th></th>}
                    <For each={data.columns}>{col =>
                        <th>{col}</th>
                    }</For>
                    <th>
                        <button type='button' onclick={e => prompt('Column Name')} title='Add a Column'>
                            <AddIcon />
                            <span class='sr-only'>Add Column</span>
                        </button>
                    </th>
                </tr>
            </thead>
            <tbody>
                <For each={data.rows ?? []}>{row =>
                    <tr>
                        {typeof rowLeader === 'function' && <td>{rowLeader(row)}</td>}
                        <For each={data.columns}>{(_, columnIndex) =>
                            <td>{row[columnIndex()]}</td>
                        }</For>
                    </tr>
                }</For>
            </tbody>
        </table>
    );
}