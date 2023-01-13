import { RouteDataFuncArgs, useParams, useRouteData } from "@solidjs/router";
import { createResource, Show } from "solid-js";
import { postFetchInode } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { FetchInodeResult } from "../../bindings/GiantTeam.Organization.Services";
import { DataResponseResource } from "../../helpers/DataResponseResource";
import { MainLayout } from "../../partials/MainLayout";
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
    const inode = useInodeRouteData();
    const params = useParams<{ organization: string }>();

    return <MainLayout>
        <Show when={inode.data?.inode}>{() => {
            return <>
                <Explorer organizationId={params.organization} inode={inode.data!.inode} />
            </>
        }}</Show>
    </MainLayout>
}