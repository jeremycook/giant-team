import { Switch, Match } from "solid-js";
import { postCreateSpace } from "../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { useOrganizationDetailsContext } from "../pages/organization/partials/OrganizationDetailsProvider";
import { toast } from "../partials/Toasts";
import { AppInfo } from "./AppInfo";
import { AppProps } from "./AppProps";

export function SpaceApp(props: AppProps) {
    let ref: HTMLInputElement = null as any;
    const organizationDetails = useOrganizationDetailsContext();

    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        const response = await postCreateSpace({
            organizationId: props.organization.organizationId,
            spaceName: ref.value,
            accessControlList: [/*TODO*/]
        });

        if (response.ok) {
            // TODO: refresh inode
            ref.value = '';
            toast.success('Space created!');
            // TODO: close self?
        }
    }

    return <>
        <Switch fallback={<>
            Spaces cannot be created here.
        </>}>
            <Match when={props.initialInode.inodeTypeId === InodeTypeId.Space}>
                TODO: Space view/editor
            </Match>
            <Match when={organizationDetails.inodeTypes[props.initialInode.inodeTypeId].allowedChildNodeTypeIds.includes(InodeTypeId.Space)}>
                <form class='flex flex-col gap-1' onsubmit={onSubmitForm}>
                    <input ref={ref} required pattern='^[^<>:"/\|?*]+$' title='Cannot contain ^ < > : " / \ | ? * ] + or $ characters.' />
                    <div>
                        TODO: grants picker
                    </div>
                    <div>
                        <button class='button'>
                            Create
                        </button>
                    </div>
                </form>
            </Match>
        </Switch>
    </>
}

export const SpaceAppInfo: AppInfo = {
    name: 'Space',
    component: SpaceApp,
    canHandle: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Space,
    showInAppDrawer: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Space || inode.inodeTypeId === InodeTypeId.Root,
}

export default SpaceAppInfo;