import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models";
import { OrganizationDetails } from "../pages/organization/OrganizationDetailsResource";


export interface AppProps {
    organization: OrganizationDetails;
    inode: Inode;
}
