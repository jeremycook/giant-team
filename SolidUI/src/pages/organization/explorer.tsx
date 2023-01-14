import { RouteDataFuncArgs, useRouteData } from "@solidjs/router";
import { createResource } from "solid-js";
import { postFetchInode } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { FetchInodeResult } from "../../bindings/GiantTeam.Organization.Services";
import { DataResponseResource } from "../../helpers/DataResponseResource";
import { ShowItem } from "../../widgets/ShowItem";
import { useOrganizationRouteData } from "./organization";
import { Explorer } from "./partials/Explorer";

export class InodeRouteData extends DataResponseResource<FetchInodeResult>{ }

export function createInodeRouteData({ params }: RouteDataFuncArgs) {
    const resourceReturn = createResource(
        () => ({ organizationId: params.organization, path: params.path }),
        (props) => postFetchInode(props)
    );
    return new InodeRouteData(resourceReturn);
}

export function useInodeRouteData() {
    return useRouteData<InodeRouteData>();
}

export default function ExplorerPage() {
    const org = useOrganizationRouteData();
    const inode = useInodeRouteData();

    return <ShowItem when={org.data}>{org =>
        <ShowItem when={inode.data?.inode}>{inode =>
            <Explorer organization={org} inode={inode} />
        }</ShowItem>
    }</ShowItem>
}