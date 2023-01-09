import { createResource, For, JSX } from "solid-js";
import { postQueryDatabase } from "../../../bindings/GiantTeam.Organization.Api.Controllers";
import { objectifyTabularData } from "../../../helpers/objectHelpers";
import { sql } from "../../../helpers/sqlHelpers";
import { camelCase } from "../../../helpers/textHelpers";
import { OrganizationCard, OrganizationModel } from "./OrganizationCard";

export function createMyOrganizationsResource() {
    const [resource, { refetch }] = createResource(() => postQueryDatabase({
        databaseName: 'directory',
        sql: sql`
            SELECT *
            FROM directory.organization
            ORDER BY name
        `.text
    }));
    return { resource, refetch };
}

export default function MyOrganizations(props: { children?: (model: OrganizationModel) => JSX.Element }) {
    const { resource } = createMyOrganizationsResource();

    const records = () => {
        const response = resource();
        if (response?.ok) {
            return objectifyTabularData<OrganizationModel>(response.data, camelCase);
        }
        else {
            return []
        }
    }

    const itemRenderer = props.children ?? (data => <OrganizationCard model={data} />)

    return () => <For each={records()}>{itemRenderer}</For>
}