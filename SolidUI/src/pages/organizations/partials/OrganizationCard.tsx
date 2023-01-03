import { A } from "../../../partials/Nav";

export interface OrganizationData {
    id: string,
    name: string,
}

export default function OrganizationCard(props: { data: OrganizationData }) {
    return (
        <div class='card'>
            <A href={'/organizations/' + props.data.id}>{props.data.name}</A>
        </div>
    )
}