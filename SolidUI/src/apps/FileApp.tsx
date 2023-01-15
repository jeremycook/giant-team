import { createSignal, Match, Show, Switch } from "solid-js"
import { AppInfo, AppProps } from "."
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { hrefOf } from "../helpers/links";
import { log } from "../helpers/logging";
import { toast } from "../partials/Toasts";

export function FileApp(props: AppProps) {
    let ref: HTMLInputElement = null as any;

    const [showRetry, setShowRetry] = createSignal(false);

    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        if (!ref.files?.length) {
            return;
        }

        const formData = new FormData();
        formData.set('organizationid', props.organization.organizationId);
        formData.set('path', props.inode.path);
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
            <Match when={props.inode.inodeTypeId === InodeTypeId.File}>
                TODO: File viewer/editor
            </Match>
            <Match when={props.inode.childrenConstraints.some(c => c.inodeTypeId === InodeTypeId.File)}>
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
    canHandle: (inode: Inode) => inode.inodeTypeId === 'File',
    component: FileApp,
}

export default FileAppInfo;