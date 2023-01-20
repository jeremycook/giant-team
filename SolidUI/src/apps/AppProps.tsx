import { Inode, OrganizationDetails } from "../bindings/GiantTeam.Organization.Etc.Models";
import { InodeExplorer } from "../pages/organization/partials/InodeExplorerContext";


export interface AppProps {
    organization: OrganizationDetails;
    explorer: InodeExplorer,
    inode: Inode;
}
