import { FetchOrganizationOutput } from "../../../bindings/GiantTeam.Cluster.Directory.Services";
import { Anchor } from "../../../partials/Anchor";

export interface OrganizationModel extends FetchOrganizationOutput {
}

export function OrganizationCard(props: { model: OrganizationModel }) {
    return (
        <div class='card'>
            <Anchor href={'/organizations/' + props.model.organizationId}>{props.model.name}</Anchor>
        </div>
    )
}