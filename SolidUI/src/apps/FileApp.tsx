import { createSignal, Match, Show, Switch } from "solid-js"
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { hrefOf } from "../helpers/links";
import { log } from "../helpers/logging";
import { useOrganizationDetailsContext } from "../pages/organization/partials/OrganizationDetailsProvider";
import { toast } from "../partials/Toasts";
import { AppInfo } from "./AppInfo";
import { AppProps } from "./AppProps";

export function FileApp(props: AppProps) {
    let ref: HTMLInputElement = null as any;
    const organizationDetails = useOrganizationDetailsContext();
    const [showRetry, setShowRetry] = createSignal(false);

    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        if (!ref.files?.length) {
            return;
        }

        const formData = new FormData();
        formData.set('organizationid', props.organization.organizationId);
        formData.set('path', props.initialInode.path);
        Object.values(ref.files)
            .forEach(f => formData.append('files', f));

        setShowRetry(false);

        try {
            // TODO: Show upload progress
            const response = await fetch(hrefOf.uploadApi, {
                method: "post",
                body: formData,
            });

            if (response.ok) {
                // TODO: refresh inode
                ref.value = '';
                toast.success('The upload succeeded!');
                // TODO: close self?
            }
            else {
                toast.error('Something went wrong.');
                setShowRetry(true);
            }
        }
        catch (error) {
            log.error('An error occurred while uploading files: {Error}', error as any);
            toast.error('Something went wrong.');
            setShowRetry(true);
        }
    };
    return <>
        <Switch fallback={<>
            Files cannot be uploaded here.
        </>}>
            <Match when={props.initialInode.inodeTypeId === InodeTypeId.File}>
                TODO: File viewer/editor
            </Match>
            <Match when={organizationDetails.inodeTypes[props.initialInode.inodeTypeId].allowedChildNodeTypeIds.includes(InodeTypeId.File)}>
                <form class='flex gap-1' onsubmit={onSubmitForm}>
                    <input ref={ref} type='file' multiple required onchange={e => e.currentTarget.form?.requestSubmit()} />
                    <Show when={showRetry()}>
                        <button class='button'>
                            Retry
                        </button>
                    </Show>
                </form>
            </Match>
        </Switch>
    </>
}

export const FileAppInfo: AppInfo = {
    name: "File",
    canHandle: (inode: Inode) => inode.inodeTypeId === InodeTypeId.File,
    showInAppDrawer: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Folder || inode.inodeTypeId === InodeTypeId.Space,
    component: FileApp,
}

export default FileAppInfo;