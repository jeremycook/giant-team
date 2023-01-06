import { createResource, Show } from "solid-js";
import { postFetchOrganization } from "../../../bindings/GiantTeam.Cluster.Api.Controllers";
import { Organization } from "../../../bindings/GiantTeam.Organizations.Directory.Models";
import { OkDataResponse } from "../../../helpers/httpHelpers";
import { here, PageSettings } from "../../../partials/Nav"
import { isAuthenticated } from "../../../utils/session"

export const pageSettings: PageSettings = {
    name: () => {
        const { organization } = here.routeValues as { organization: string };
        return organization;
    },
    showInNav: () => isAuthenticated() && 'organization' in here.routeValues,
}

export function createOrganizationResource() {
    const [resource, { refetch }] = createResource(
        () => ({ organization: here.routeValues.organization as string }),
        async (props) => await postFetchOrganization({
            organizationId: props.organization
        })
    );

    return { resource, refetch };
}

export default function OrganizationPage() {
    const { resource } = createOrganizationResource();

    return <>
        <Show when={resource.loading}>
            Loading...
        </Show>
        <Show when={resource()?.ok}>{() => {
            const data = (resource() as OkDataResponse<Organization>).data;
            return <>
                <h1>{data.name}</h1>

                <h2>Spaces</h2>
                {/* TODO: <For each={data.spaces}>{schema => <SpaceCard data={{ id: schema.name, name: schema.name, schema: schema }} />}</For> */}
            </>
        }}</Show>
    </>
}