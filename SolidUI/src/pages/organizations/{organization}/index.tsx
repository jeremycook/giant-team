import { createResource, For, Show } from "solid-js";
import { Schema } from "../../../bindings/GiantTeam.DatabaseDefinition.Models";
import { postQueryDatabase } from "../../../bindings/GiantTeam.Organization.Api.Controllers";
import { camelCasePropertyNames, objectifyTabularData, parseJson } from "../../../helpers/objectHelpers";
import { sql } from "../../../helpers/sqlHelpers";
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
        (props) => postQueryDatabase({
            databaseName: props.organization,
            sql: sql`
                SELECT *
                FROM spaces.database_definition
                ORDER BY name
            `.text
        }));
    return { resource, refetch };
}

export default function OrganizationPage() {
    const { resource } = createOrganizationResource();

    const organization = () => {
        const response = resource();
        if (response?.ok) {
            const data = objectifyTabularData(response.data)[0] as {
                name: string,
                owner: string,
                schemas: string
            };
            return {
                name: data.name,
                owner: data.owner,
                schemas: camelCasePropertyNames(parseJson(data.schemas)) as Schema[],
            };
        }
        else {
            return undefined
        }
    }

    return <>
        <Show when={resource.loading}>
            Loading...
        </Show>
        <Show when={resource()?.ok}>{() => {
            const org = organization()!;

            return <>
                    {JSON.stringify(org)}
                <h1>{org.name}</h1>

                <h2>Apps</h2>
                <p>TODO</p>

                <h2>Spaces</h2>
                <For each={org.schemas} fallback={<>No items</>}>{schema => <>
                    <SpaceCard data={{ id: schema.name, name: schema.name, schema: schema }} />
                </>}</For>
            </>
        }}</Show>
    </>
}