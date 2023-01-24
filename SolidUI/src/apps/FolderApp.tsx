import { Switch, Match } from "solid-js";
import { postCreateFolder } from "../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { useOrganizationDetailsContext } from "../pages/organization/partials/OrganizationDetailsProvider";
import { toast } from "../partials/Toasts";
import { AppInfo } from "./AppInfo";
import { AppProps } from "./AppProps";

export function FolderApp(props: AppProps) {
    let ref: HTMLInputElement = null as any;
    const organizationDetails = useOrganizationDetailsContext();

    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        const response = await postCreateFolder({
            organizationId: props.organization.organizationId,
            parentInodeId: props.initialInode.inodeId,
            folderName: ref.value,
            access: []
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
            <Match when={props.initialInode.inodeTypeId === InodeTypeId.Folder}>
                TODO: Folder view/editor
            </Match>
            <Match when={organizationDetails.inodeTypes[props.initialInode.inodeTypeId].allowedChildNodeTypeIds.includes(InodeTypeId.Folder)}>
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
    name: 'Folder',
    component: FolderApp,
    canHandle: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Folder,
    showInAppDrawer: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Folder || inode.inodeTypeId === InodeTypeId.Space,
}

export default FolderAppInfo;
