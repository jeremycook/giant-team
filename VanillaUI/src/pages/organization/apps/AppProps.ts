import { OrganizationDetails } from '../../../bindings/Organization.Etc.Models';
import { InodeProvider } from '../_logic/InodeProvider';
import { Process } from '../_logic/ProcessOperator';

export interface AppProps {
    organization: OrganizationDetails;
    inodeProvider: InodeProvider;
    process: Process;
}
