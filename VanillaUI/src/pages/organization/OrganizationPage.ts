import { h } from '../../helpers/h';

export default function OrganizationPage({ routeValues }: { routeValues: { organization: string } }) {
    const root = h('div', null, routeValues.organization)
    return root;
}