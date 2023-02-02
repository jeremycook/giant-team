import { OrganizationDetails } from '../../../bindings/Organization.Etc.Models';
import { h } from '../../../helpers/h';
import { InodeProvider } from '../_logic/InodeProvider';
import { ProcessOperator } from '../_logic/ProcessOperator';

export function Organization(props: {
    organization: OrganizationDetails,
    processOperator: ProcessOperator,
    inodeProvider: InodeProvider
}) {
    return h('div', props.organization.rootInode.name);
}