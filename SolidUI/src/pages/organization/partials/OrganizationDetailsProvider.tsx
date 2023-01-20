import { createContext, ParentProps, useContext } from "solid-js";
import { OrganizationDetails } from "../../../bindings/GiantTeam.Organization.Etc.Models";


export const OrganizationDetailsContext = createContext(undefined as unknown as OrganizationDetails);

export function useOrganizationDetailsContext() { return useContext(OrganizationDetailsContext); }

export function OrganizationDetailsProvider(props: { organizationDetails: OrganizationDetails } & ParentProps) {
    return (
        <OrganizationDetailsContext.Provider value={props.organizationDetails}>
            {props.children}
        </OrganizationDetailsContext.Provider>
    );
}