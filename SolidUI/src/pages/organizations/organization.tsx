import { RouteDataFuncArgs, useRouteData } from "@solidjs/router";
import { createResource, Show } from "solid-js";
import { postFetchOrganizationDetails } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { FetchOrganizationDetailsResult } from "../../bindings/GiantTeam.Organization.Services";
import { DataResponseResource } from "../../helpers/DataResponseResource";
import { Explorer } from "./{organization}/partials/Explorer";

export class OrganizationRouteData extends DataResponseResource<FetchOrganizationDetailsResult>{ }

export function createOrganizationRouteData({ params }: RouteDataFuncArgs) {
    const resourceReturn = createResource(
        () => ({ organizationId: params.organization }),
        (props) => postFetchOrganizationDetails(props)
    );
    return new OrganizationRouteData(resourceReturn);
}

export function useOrganizationRouteData() {
    return useRouteData<OrganizationRouteData>();
}

export default function OrganizationPage() {
    const routeData = useOrganizationRouteData();

    return <>
        <Show when={routeData.data}>{() => {
            return <>
                <h1>{routeData.data!.name}</h1>
                <Explorer organizationId={routeData.data!.organizationId} path='/' />
            </>
        }}</Show>
    </>
}