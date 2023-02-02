import { postFetchOrganizations } from '../../../bindings/Cluster.Api.Controllers';
import { Organization } from '../../../bindings/Cluster.Directory.Models';
import { f, BaseNode } from '../../../helpers/h';
import { OrganizationCard } from './OrganizationCard';


export default async function MyOrganizations(renderer?: (organization: Organization) => BaseNode) {

    const response = await postFetchOrganizations();

    const organizations = response.ok
        ? response.data.organizations
        : [];

    if (typeof renderer === 'undefined') {
        renderer = ((organization: Organization) => OrganizationCard({ organization }));
    }

    const elements = organizations.flatMap(org => renderer!(org));
    const fragment = f(...elements);

    return fragment;
}