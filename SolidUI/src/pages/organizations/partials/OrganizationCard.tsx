import { Organization } from "../../../bindings/GiantTeam.Organizations.Directory.Models";
import { A } from "../../../partials/Nav";

export interface OrganizationModel extends Organization {
}

export function OrganizationCard(props: { model: OrganizationModel }) {
    return (
        <div class='card'>
            <A href={'/organizations/' + props.model.organizationId}>{props.model.name}</A>
        </div>
    )
}