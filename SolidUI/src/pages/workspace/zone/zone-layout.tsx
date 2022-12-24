import { A, Outlet, useParams, useRouteData } from "@solidjs/router";
import { combinePaths } from "../../../helpers/urlHelpers";
import { Breadcrumb } from "../../../utils/nav";
import { createWorkspaceUrl, WorkspaceLayoutData } from "../workspace-layout";

export const useZoneParams = () => {
    const params = useParams();
    return {
        workspace: params.workspace,
        zone: params.zone,
    };
};

export function createZoneUrl() {
    const params = useZoneParams();
    return combinePaths(createWorkspaceUrl(), `zone/${params.zone}`);
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

    // const breadcrumbLink = () => {
    //     return zoneData() ? { title: zoneData()!.name, url: './', } : undefined
    // };
    // const breadcrumbs = useBreadcrumbContext();

    // const navLink = createMutable({
    //     title: 'Loading zone…',
    //     url: './',
    // })


    // createEffect(() => {
    //     const zd = zoneData();
    //     if (zd) {
    //         breadcrumbs.push(navLink);
    //     }
    //     else    {

    //     }
    // });

    // onCleanup(() => {
    //     breadcrumbs.remove(navLink);
    // });

    return (<>

        <Breadcrumb link={{ text: zoneData()!.name, href: createZoneUrl() }} />

        <div class='flex gap-1 children:button'>
            <A href={'../../'}>Workspace</A>
            <A href={'./import-data'}>Import Data</A>
            <A href={'./table-maker'}>Add Table</A>

            {/* <div class='dropdown'>
                <button type='button' class='dropdown-button'>
                    Schemas
                </button>
                <div class='dropdown-anchor'>
                    <div class='dropdown-content stack'>
                        <For each={workspace()!.zones}>{(zone =>
                            <A href={`./zone/${zone.name}`}>{zone.name}</A>
                        )}</For>
                    </div>
                </div>
            </div> */}
        </div>

        <Outlet />

    </>)
}