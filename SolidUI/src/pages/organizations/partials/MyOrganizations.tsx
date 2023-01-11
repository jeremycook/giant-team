import { createResource, For, JSX } from "solid-js";
import { postQueryDatabase } from "../../../bindings/GiantTeam.Organization.Api.Controllers";
import { DataResponseResource } from "../../../helpers/DataResponseResource";
import { objectifyTabularData } from "../../../helpers/objectHelpers";
import { sql } from "../../../helpers/sqlHelpers";
import { camelCase } from "../../../helpers/textHelpers";
import { OrganizationCard, OrganizationModel } from "./OrganizationCard";

export function createMyOrganizationsResource() {
    const resourceReturn = createResource(() => postQueryDatabase({
        databaseName: 'directory',
        sql: sql`
            SELECT *
            FROM directory.organization
            ORDER BY name
        `.text
    }));
    return new DataResponseResource(resourceReturn);
}

export default function MyOrganizations(props: { children?: (model: OrganizationModel) => JSX.Element }) {
    const orgs = createMyOrganizationsResource();

    const records = () => {
        if (orgs.data) {
            return objectifyTabularData<OrganizationModel>(orgs.data, camelCase);
        }
        else {
            return []
        }
    }

    const itemRenderer = props.children ?? (data => <OrganizationCard model={data} />)

    return () => <For each={records()}>{itemRenderer}</For>
}