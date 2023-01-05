import { createResource, For, JSX } from "solid-js";
import { postQueryDatabase } from "../../../api/GiantTeam.Data.Api.Controllers";
import { convertTabularDataToObjects } from "../../../helpers/objectHelpers";
import { sql } from "../../../helpers/sqlHelpers";
import { OrganizationCard, OrganizationModel } from "./OrganizationCard";

export function createMyOrganizationsResource() {
    const [resource, { refetch }] = createResource(() => postQueryDatabase({
        databaseName: 'directory',
        sql: sql`
            SELECT *
            FROM directory.organizations
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
            return convertTabularDataToObjects<OrganizationModel>(response.data);
        }
        else {
            return []
        }
    }

    const itemRenderer = props.children ?? (data => <OrganizationCard model={data} />)

    return () => <For each={records()}>{itemRenderer}</For>
}