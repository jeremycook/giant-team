import { Inode, OrganizationDetails } from "../bindings/GiantTeam.Organization.Etc.Models";
import { InodeProvider } from "../pages/organization/partials/InodeProvider";


export interface AppProps {
    organization: OrganizationDetails;
    inodeProvider: InodeProvider,
    initialInode: Inode;
}
