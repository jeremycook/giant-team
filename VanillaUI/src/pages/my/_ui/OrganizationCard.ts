import { Organization } from '../../../bindings/Cluster.Directory.Models';
import { h } from '../../../helpers/h';

export function OrganizationCard({ organization }: { organization: Organization }) {
    return h('.card',
        h('a', x => x.set({ href: '/o/' + organization.organizationId }),
            organization.name
        )
    )
}