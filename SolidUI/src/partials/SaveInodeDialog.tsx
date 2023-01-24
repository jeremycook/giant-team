import { createEffect, For, JSX } from "solid-js";
import { createStore } from "solid-js/store";
import { Inode, InodeAccess, InodeTypeId, PermissionId } from "../bindings/GiantTeam.Organization.Etc.Models";
import { immute } from "../helpers/immute";
import { InodeProvider } from "../pages/organization/partials/InodeProvider";
import { InodeRoot } from "../pages/organization/partials/InodeTree";
import Dialog from "../widgets/Dialog";

const permisions = [
    { id: PermissionId.r, name: 'Read', description: '' },
    { id: PermissionId.a, name: 'Append', description: '' },
    { id: PermissionId.w, name: 'Write', description: '' },
    { id: PermissionId.d, name: 'Delete', description: '' },
    { id: PermissionId.m, name: 'Manage', description: '' },
];

export function SaveInodeDialog(props: {
    type: InodeTypeId,
    inodeProvider: InodeProvider,
    initialInode: Inode,
    onDismiss: JSX.EventHandlerUnion<HTMLButtonElement, MouseEvent>,
    onSubmit: (
        e: SubmitEvent & { currentTarget: HTMLFormElement },
        model: { parentInode: Inode, name: string; accessControls: InodeAccess[]; }
    ) => void
}) {
    const [model, setModel] = createStore({
        parentInode: props.initialInode,
        name: '',
        accessControls: props.inodeProvider.organization.roles.map(r => ({
            roleId: r.roleId,
            name: r.name,
            permissions: r.name === 'Owner' ? [PermissionId.r, PermissionId.a, PermissionId.w, PermissionId.d, PermissionId.m] :
                r.name === 'Admin' ? [PermissionId.r, PermissionId.a, PermissionId.w, PermissionId.d] :
                    [PermissionId.r],
        })),
    });

    createEffect(() => {
        console.log(JSON.stringify(model.accessControls));
    })

    return <>
        <Dialog title='Save As' onDismiss={props.onDismiss}>
            <form onsubmit={e => props.onSubmit(e, model)}>
                <div class='flex flex-col gap-1'>
                    <div class='h-200px b b-solid pxy'>
                        <InodeRoot
                            inodeProvider={props.inodeProvider}
                            selectedInode={() => model.parentInode}
                            onClickInode={(e, inode, { isExpanded, expand }) => {
                                setModel('parentInode', inode);
                                expand(true);
                            }} />
                    </div>
                    <input
                        value={model.name}
                        oninput={e => setModel('name', e.currentTarget.value ?? '')}
                        required
                        placeholder={props.type + ' Name'} />
                    <div class='b b-solid pxy'>
                        <table>
                            <thead>
                                <tr>
                                    <th>Role</th>
                                    <For each={permisions}>{p => <>
                                        <th title={p.description}>{p.name}</th>
                                    </>}</For>
                                </tr>
                            </thead>
                            <tbody>
                                <For each={model.accessControls}>{(accessControl, roleIndex) => <>
                                    <tr>
                                        <td>{accessControl.name}</td>
                                        <For each={permisions}>{p => <>
                                            <td><input type='checkbox' value={p.id}
                                                checked={accessControl.permissions.includes(p.id)}
                                                onchange={e => {
                                                    if (accessControl.permissions.includes(p.id)) {
                                                        setModel('accessControls', roleIndex(), 'permissions', x => immute.remove(x, p.id));
                                                    }
                                                    else {
                                                        setModel('accessControls', roleIndex(), 'permissions', x => [...x, p.id]);
                                                    }
                                                }} /></td>
                                        </>}</For>
                                    </tr>
                                </>}</For>
                            </tbody>
                        </table>
                    </div>
                    <div class='flex gap-1 justify-end'  >
                        <button class='button-primary'>Save</button>
                        <button type='button' class='button'>Cancel</button>
                    </div>
                </div>
            </form>
        </Dialog>
    </>
}