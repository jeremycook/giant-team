import { FetchOrganizationOutput } from "../../../bindings/GiantTeam.Cluster.Directory.Services";
import { A } from "../../../partials/Nav";

export interface OrganizationModel extends FetchOrganizationOutput {
}

export function OrganizationCard(props: { model: OrganizationModel }) {
    return (
        <div class='card'>
            <A href={'/organizations/' + props.model.organizationId}>{props.model.name}</A>
        </div>
    )
}