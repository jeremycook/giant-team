import { A } from "@solidjs/router";
import { For } from "solid-js";
import { createStore } from "solid-js/store";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { FetchOrganizationDetailsApp, FetchOrganizationDetailsResult } from "../../../bindings/GiantTeam.Organization.Services";
import { removeItem } from "../../../helpers/arrayHelpers";
import { hrefOf } from "../../../helpers/links";
import Dialog from "../../../widgets/Dialog";

export function Explorer(props: { organization: FetchOrganizationDetailsResult, inode: Inode }) {

    const [processes, setProcesses] = createStore<{ app: FetchOrganizationDetailsApp, inode: Inode }[]>([]);

    const segments = () => props.inode.path.length > 0 ?
        props.inode.path.split('/').reduce((prev, name) => ([{ name, path: prev[prev.length - 1]?.path + '/' + name }, ...prev]), [] as { name: string, path: string }[]) :
        [];

    const launchApp = (app: FetchOrganizationDetailsApp, inode: Inode) => {
        setProcesses(x => [...x, { app, inode }]);
    };

    return <>
        <div class='flex'>
            <div class='dropdown'>
                <button class='dropdown-button' type='button' id='Explorer-apps-dropdown'>
                    {/* <PersonIcon class='parent-active' /> */}
                    {/* <PersonOutlineIcon class='parent-inactive' /> */}
                    Apps
                    <span class='md:sr-only'> Apps</span>
                </button>
                <div class='dropdown-anchor' aria-labelledby='Explorer-apps-dropdown'>
                    <div class='dropdown-content stack'>
                        <For each={props.organization.apps}>{app => <>
                            <button type='button' class='stack-item' onclick={() => { launchApp(app, props.inode); }}>
                                {app.name}
                            </button>
                        </>}</For>
                    </div>
                </div>
            </div>

            <div>
                <A href={hrefOf.inode(props.organization.organizationId, '')}>{props.organization.name}</A>
                <For each={segments()}>{segment => <>
                    {' / '}
                    <A href={hrefOf.inode(props.organization.organizationId, segment.path)}>{segment.name}</A>
                </>}</For>
            </div>
        </div>

        <div class='flex flex-wrap gap-4'>
            <For each={props.inode.children}>{child => (<>
                <div class='card'>
                    <A href={hrefOf.inode(props.organization.organizationId, child.path)}>{child.name}</A>
                </div>
            </>)}</For>
        </div>

        <For each={processes} fallback='Nothing'>{process => <Dialog
            title={process.app.name + ': ' + process.inode.name}
            onDismiss={() => setProcesses(x => { removeItem(x, process); return x; })}
        >
            {JSON.stringify(process)}
        </Dialog>}</For>
    </>
}