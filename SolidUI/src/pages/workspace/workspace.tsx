import { A, Outlet } from "@solidjs/router";
import { For } from "solid-js";
import { createId } from "../../helpers/htmlHelpers";
import { useWorkspaceRouteData } from "./workspace-layout";

export default function WorkspacePage() {
    const workspaceRouteData = useWorkspaceRouteData();

    const workspace = () => {
        const ws = workspaceRouteData();
        return ws?.ok ? ws.data : null;
    };

    return (<>

        <div class='flex gap-1 children:button'>
            <A href={'./import-data'}>Import Data</A>
            <A href={'./create-schema'}>Add Schema</A>

            <div class='dropdown'>
                <button type='button' class='dropdown-button' id={createId('SchemasDropdown')}>
                    Schemas
                </button>
                <div class='dropdown-anchor' aria-labelledby={createId('SchemasDropdown')}>
                    <div class='dropdown-content stack'>
                        <For each={workspace()!.zones}>{(zone =>
                            <A href={`./zone/${zone.name}`}>{zone.name}</A>
                        )}</For>
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