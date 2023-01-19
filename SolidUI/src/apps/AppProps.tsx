import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models";
import { OrganizationDetails } from "../pages/organization/OrganizationDetailsResource";
import { InodeExplorer } from "../pages/organization/partials/InodeExplorerContext";


export interface AppProps {
    organization: OrganizationDetails;
    explorer: InodeExplorer,
    inode: Inode;
}
