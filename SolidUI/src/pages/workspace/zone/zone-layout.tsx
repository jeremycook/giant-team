import { A, Outlet, useParams, useRouteData } from "@solidjs/router";
import { For } from "solid-js";
import { Breadcrumb } from "../../../utils/nav";
import { createWorkspaceUrl, WorkspaceLayoutData } from "../workspace-layout";

export const useZoneParams = () => {
    const params = useParams();
    return {
        workspace: params.workspace,
        zone: params.zone,
    };
};

export function createZoneUrl(...paths: string[]) {
    const params = useZoneParams();
    return createWorkspaceUrl(`zone/${params.zone}`, ...paths);
}

export const useZoneData = () => {
    const params = useParams();
    const workspaceRouteData = useRouteData<typeof WorkspaceLayoutData>();

    return () => {
        const zoneName = params.zone;
        const wsrd = workspaceRouteData();
        return wsrd?.ok ? {
            workspace: wsrd.data,
            ...wsrd.data.zones.find(z => z.name === zoneName)!,
        } : undefined;
    }
}

export default function ZoneLayout() {
    const zoneData = useZoneData();

    return (<>

        <Breadcrumb link={{ text: zoneData()!.name, href: createZoneUrl() }} />

        <div class='flex gap-1'>
            <A class='button' href={createZoneUrl('import-data')}>Import Data</A>
            <div class='dropdown'>
                <button type='button' class='dropdown-button button'>
                    Tables
                </button>
                <div class='dropdown-anchor'>
                    <div class='dropdown-content card p-1 children:p-1 children:max-w-sm children:truncate'>
                        <For each={zoneData()!.tables}>{(table =>
                            <A href={createZoneUrl('table', table.name)}>{table.name}</A>
                        )}</For>
                        <A href={createZoneUrl('new-table')}>New Table</A>
                    </div>
                </div>
            </div>
        </div>

        <Outlet />

    </>)
}