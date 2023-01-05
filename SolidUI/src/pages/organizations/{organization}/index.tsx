import { createResource, For, Show } from "solid-js";
import { postFetchWorkspace } from "../../../bindings/GiantTeam.Data.Api.Controllers";
import { Workspace } from "../../../bindings/GiantTeam.Workspaces.Models";
import { OkDataResponse } from "../../../helpers/httpHelpers";
import { here, PageSettings } from "../../../partials/Nav"
import { isAuthenticated } from "../../../utils/session"
import SpaceCard from "./partials/SpaceCard";

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
        async (props) => await postFetchWorkspace({
            workspaceName: props.organization
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
            const data = (resource() as OkDataResponse<Workspace>).data;
            return <>
                <h1>{data.name}</h1>

                <h2>Spaces</h2>
                <For each={data.zones}>{schema => <SpaceCard data={{ id: schema.name, name: schema.name, schema: schema }} />}</For>
            </>
        }}</Show>
    </>
}