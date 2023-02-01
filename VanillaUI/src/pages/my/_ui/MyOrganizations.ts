import { postFetchOrganizations } from '../../../bindings/Cluster.Api.Controllers';
import { Organization } from '../../../bindings/Cluster.Directory.Models';
import { f, BaseNode } from '../../../helpers/h';
import { OrganizationCard } from './OrganizationCard';


export default function MyOrganizations(renderer?: (organization: Organization) => BaseNode) {

    const ref = document.createComment('');

    const response = postFetchOrganizations()
        .then(response => {
            const organizations = response.ok
                ? response.data.organizations
                : [];

            if (typeof renderer === 'undefined') {
                renderer = ((organization: Organization) => OrganizationCard({ organization }));
            }

            const elements = organizations.flatMap(org => renderer!(org));
            const fragment = f(...elements);
            ref.after(fragment);
        });

    return ref;
}