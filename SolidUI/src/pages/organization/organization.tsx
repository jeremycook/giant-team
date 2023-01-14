import { RouteDataFuncArgs, useRouteData } from "@solidjs/router";
import { createResource } from "solid-js";
import { postFetchOrganizationDetails } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { FetchOrganizationDetailsResult } from "../../bindings/GiantTeam.Organization.Services";
import { DataResponseResource } from "../../helpers/DataResponseResource";
import { ShowItem } from "../../widgets/ShowItem";
import { Explorer } from "./partials/Explorer";

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
    const org = useOrganizationRouteData();

    return <ShowItem when={org.data}>{org =>
        <Explorer organization={org} inode={org.inode} />
    }</ShowItem>
}