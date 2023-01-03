import { createResource, For, JSX } from "solid-js";
import { postFetchRecords } from "../../../api/GiantTeam.Data.Api";
import OrganizationCard, { OrganizationData } from "./OrganizationCard";

export function createMyOrganizationsResource() {
    const [resource, { refetch }] = createResource(() => postFetchRecords({
        database: 'Home',
        schema: 'my',
        table: 'organizations',
        columns: null,
        filters: null,
        skip: null,
        take: null,
        verbose: null,
    }));
    return { resource, refetch };
}

export default function MyOrganizations(props: { children?: (data: OrganizationData) => JSX.Element }) {
    const { resource } = createMyOrganizationsResource();

    const records = () => {
        const response = resource();
        if (response?.ok) {
            return response.data.records.map(rec =>
                response.data.columns.reduce((obj, col, i) => ({
                    ...obj,
                    [col.name]: rec[i],
                }), {}) as OrganizationData
            );
        }
        else {
            return []
        }
    }

    const itemRenderer = props.children ?? (data => <OrganizationCard data={data} />)

    return () => <For each={records()}>{itemRenderer}</For>
}