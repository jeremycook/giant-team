import { Switch, Match, createSignal, For, Show } from "solid-js";
import { StoreType } from "../../bindings/GiantTeam.DatabaseDefinition.Models";
import { postCreateTable } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, InodeTypeId } from "../../bindings/GiantTeam.Organization.Etc.Models"
import { AddOutlineIcon } from "../../partials/Icons";
import { OpenInodeDialog } from "../../partials/OpenInodeDialog";
import { SaveInodeDialog } from "../../partials/SaveInodeDialog";
import { toast } from "../../partials/Toasts";
import { ShowItem } from "../../widgets/ShowItem";
import { AppInfo } from "../AppInfo";
import { AppProps } from "../AppProps";
import { TableManager } from "./TableManager";

enum DialogState {
    Closed,
    Open,
    Save,
}

export function TableApp(props: AppProps) {
    const [dialogState, setDialogState] = createSignal(DialogState.Closed);

    const tableManager = new TableManager({
        organization: props.organization,
        inode: props.process.inode,
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
                    <ManagedTable tableManager={tableManager} />
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

function ManagedTable(props: { tableManager: TableManager }) {
    return <Show when={props.tableManager.definition}><ShowItem when={props.tableManager.table}>{table =>
        <table>
            <thead>
                <tr>
                    <th>
                        <input type='checkbox' />
                    </th>
                    <For each={table.columns}>{col =>
                        <Show when={props.tableManager.data.columns.includes(col.name)}>
                            <th>{col.name}</th>
                        </Show>
                    }</For>
                    <th>
                        <button type='button' onclick={e => {

                            const columnName = prompt('Column Name');
                            if (!columnName) return;

                            props.tableManager.createColumn({
                                name: columnName,
                                storeType: StoreType.Text,
                                defaultValueSql: '',
                                isNullable: false,
                                computedColumnSql: null,
                                position: -1,
                            });
                        }} title='Add a Column'>
                            <AddOutlineIcon />
                            <span class='sr-only'>Add Column</span>
                        </button>
                    </th>
                </tr>
            </thead>
            <tbody>
                <ShowItem when={props.tableManager.data}>{data => <>
                    <For each={data.rows}>{row =>
                        <tr>
                            <td>
                                <input type='checkbox' checked={row.selected} onChange={e => row.setSelected(e.currentTarget.checked)} />
                            </td>
                            <For each={table.columns}>{column =>
                                <Show when={data.columns.includes(column.name)}>
                                    <td>{row.values[data.columns.indexOf(column.name)]?.toString()}</td>
                                </Show>
                            }</For>
                        </tr>
                    }</For>
                    <tr>
                        <td colspan={table.columns.length}>
                            <button type='button'
                                onClick={_ => props.tableManager.appendEmptyRow()}>
                                Add Record
                            </button>
                        </td>
                    </tr>
                </>}</ShowItem>

            </tbody>
        </table>
    }</ShowItem></Show>
}