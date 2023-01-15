import { Switch, Match } from "solid-js";
import { AppInfo, AppProps } from "."
import { postCreateFolder } from "../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { toast } from "../partials/Toasts";

export function FolderApp(props: AppProps) {
    let ref: HTMLInputElement = null as any;

    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        const response = await postCreateFolder({
            organizationId: props.organization.organizationId,
            parentInodeId: props.inode.inodeId,
            folderName: ref.value,
        });

        if (response.ok) {
            // TODO: refresh inode
            ref.value = '';
            toast.success('Folder created!');
            // TODO: close self?
        }
    }

    return <>
        <Switch fallback={<>
            Folders cannot be created here.
        </>}>
            <Match when={props.inode.inodeTypeId === InodeTypeId.Folder}>
                TODO: Folder view/editor
            </Match>
            <Match when={props.inode.childrenConstraints.some(c => c.inodeTypeId === InodeTypeId.Folder)}>
                <form class='flex gap-1' onsubmit={onSubmitForm}>
                    <input ref={ref} required pattern='^[^<>:"/\|?*]+$' title='Cannot contain ^ < > : " / \ | ? * ] + or $ characters.' />
                    <button class='button'>
                        Create
                    </button>
                </form>
            </Match>
        </Switch>
    </>
}

export const FolderAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Folder',
    component: FolderApp,
}

export default FolderAppInfo;