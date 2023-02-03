import { fetchOrganizationDetails } from '../../bindings/Organization.Api.Controllers';
import { InodeProvider } from './_logic/InodeProvider';
import { ProcessOperator } from './_logic/ProcessOperator';
import { Organization } from './_ui/Organization';

export default async function OrganizationPage({ routeValues }: { routeValues: { organization: string } }) {

    const organization = await fetchOrganizationDetails({ organizationId: routeValues.organization });

    const inodeProvider = new InodeProvider({
        organization,
    });

    const processOperator = new ProcessOperator();

    return Organization({
        inodeProvider,
        organization,
        processOperator,
    })
}