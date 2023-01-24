import { OrganizationDetails } from "../bindings/GiantTeam.Organization.Etc.Models";
import { InodeProvider } from "../pages/organization/partials/InodeProvider";
import { Process } from "../pages/organization/partials/ProcessOperatorContext";


export interface AppProps {
    organization: OrganizationDetails;
    inodeProvider: InodeProvider;
    process: Process;
}
