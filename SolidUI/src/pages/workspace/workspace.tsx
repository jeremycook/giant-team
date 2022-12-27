import { A, Outlet } from "@solidjs/router";
import { For } from "solid-js";
import { createId } from "../../helpers/htmlHelpers";
import { createWorkspaceUrl, useWorkspaceRouteData } from "./workspace-layout";

export default function WorkspacePage() {
    const workspaceRouteData = useWorkspaceRouteData();

    const workspace = () => {
        const ws = workspaceRouteData();
        return ws?.ok ? ws.data : null;
    };

    return (<>

        <div class='flex gap-1'>
            <A class='button' href={createWorkspaceUrl('import-data')}>Import Data</A>
            <div class='dropdown'>
                <button type='button' class='dropdown-button button' id={createId('ZonesDropdown')}>
                    Zones
                </button>
                <div class='dropdown-anchor' aria-labelledby={createId('ZonesDropdown')}>
                    <div class='dropdown-content flex flex-col card p-1'>
                        <For each={workspace()!.zones}>{(zone =>
                            <A class='p-1 max-w-sm truncate' href={createWorkspaceUrl('zone', zone.name)}>{zone.name}</A>
                        )}</For>
                        <A class='p-1 max-w-sm truncate' href={createWorkspaceUrl('new-zone')}>Add Zone</A>
                    </div>
                </div>
            </div>

            <div>
                Owner: {workspace()!.owner}
            </div>
        </div>

        <Outlet />

    </>)
}