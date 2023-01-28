import { fetchInodeList, fetchOrganizationDetails } from '../../bindings/Organization.Api.Controllers';
import { InodeProvider } from './_logic/InodeProvider';
import { ProcessOperator } from './_logic/ProcessOperator';
import { Organization } from './_ui/Organization';

export default async function OrganizationPage({ routeValues }: { routeValues: { organization: string } }) {

    const organization = await fetchOrganizationDetails({ organizationId: routeValues.organization });
    const inodes = await fetchInodeList({ organizationId: routeValues.organization, path: '' });

    const inodeProvider = new InodeProvider({
        organization,
        inodes
    });

    const processOperator = new ProcessOperator();

    return Organization({
        inodeProvider,
        organization,
        processOperator,
    })
}