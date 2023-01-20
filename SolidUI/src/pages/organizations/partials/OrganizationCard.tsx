import { Organization } from "../../../bindings/GiantTeam.Cluster.Directory.Models";
import { Anchor } from "../../../partials/Anchor";


export interface OrganizationModel extends Organization { }

export function OrganizationCard(props: { model: OrganizationModel }) {
    return (
        <div class='card'>
            <Anchor href={'/o/' + props.model.organizationId}>{props.model.name}</Anchor>
        </div>
    )
}