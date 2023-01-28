import { postFetchOrganizations } from '../../../bindings/Cluster.Api.Controllers';
import { Organization } from '../../../bindings/Cluster.Directory.Models';
import { appendChild, h } from '../../../helpers/h';
import { OrganizationCard } from './OrganizationCard';


export default function MyOrganizations(props: object, children?: ((model: Organization) => JSX.Element)[]) {

    const ref = document.createComment('');

    const response = postFetchOrganizations()
        .then(response => {
            const organizations = response.ok
                ? response.data.organizations
                : [];

            const itemRenderer = children && children.length > 0
                ? children[0]
                : ((data: Organization) => <OrganizationCard organization={data} />)

            const elements = organizations.flatMap(org => itemRenderer(org));

            const temp = document.createDocumentFragment(); // h('div');
            appendChild(temp, elements);

            ref.after(temp);
        });

    return ref;
}