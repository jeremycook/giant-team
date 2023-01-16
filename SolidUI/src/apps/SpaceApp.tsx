import { Switch, Match } from "solid-js";
import { AppInfo, AppProps } from "."
import { postCreateSpace } from "../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, InodeTypeId, SchemaPermissionId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { toast } from "../partials/Toasts";

export function SpaceApp(props: AppProps) {
    let ref: HTMLInputElement = null as any;

    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        const response = await postCreateSpace({
            organizationId: props.organization.organizationId,
            spaceName: ref.value,
            accessControlList: [
                { dbRole: 'TODO', permissions: [SchemaPermissionId.R_USAGE, SchemaPermissionId.A_CREATE] }
            ]
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
            <Match when={props.inode.inodeTypeId === InodeTypeId.Space}>
                TODO: Space view/editor
            </Match>
            <Match when={props.inode.childrenConstraints.some(c => c.inodeTypeId === InodeTypeId.Space)}>
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
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Space',
    component: SpaceApp,
}

export default SpaceAppInfo;