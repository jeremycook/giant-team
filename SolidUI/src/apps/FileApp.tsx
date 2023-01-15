import { createSignal, JSX, Match, Show, Switch } from "solid-js"
import { AppInfo, AppProps } from "."
import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { hrefOf } from "../helpers/links";
import { log } from "../helpers/logging";
import { toast } from "../partials/Toasts";

export function FileApp(props: AppProps) {
    let filesRef: HTMLInputElement = null as any;

    const [errored, setErrored] = createSignal(false);


    const onSubmitForm = async (e: SubmitEvent) => {
        e.preventDefault();

        if (!filesRef.files?.length) {
            return;
        }

        const formData = new FormData();
        formData.set('organizationid', props.organization.organizationId);
        formData.set('path', props.inode.path);
        Object.values(filesRef.files)
            .forEach(f => formData.append('files', f));

        setErrored(false);

        try {
            const response = await fetch(hrefOf.uploadApi, {
                method: "post",
                body: formData,
            });

            if (response.ok) {
                toast.success('Success!');
            }
            else {
                toast.error('Something went wrong.');
                setErrored(true);
            }
        }
        catch (error) {
            log.error('An error occurred while uploading files: {Error}', error as any);
            toast.error('Something went wrong.');
            setErrored(true);
        }

    };
    return <>
        <Switch fallback={<>
            You cannot upload files here. Files can be uploaded into Folders.
        </>}>
            <Match when={props.inode.childrenConstraints.some(c => c.inodeTypeId === InodeTypeId.File)}>
                {/* Present the upload interface */}
                <form class='flex gap-1' onsubmit={onSubmitForm}>
                    <input ref={filesRef} type='file' multiple required onchange={e => e.currentTarget.form?.requestSubmit()} />
                    <Show when={errored()}>
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