import { Organization } from '../../../bindings/Cluster.Directory.Models';


export function OrganizationCard(props: { organization: Organization }) {
    return <div class='card'>
        <a href={'/o/' + props.organization.organizationId}>{props.organization.name}</a>
    </div>
}