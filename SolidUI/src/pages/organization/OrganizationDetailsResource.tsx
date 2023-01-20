import { createResource } from "solid-js";
import { postFetchOrganizationDetails } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { OrganizationDetails } from "../../bindings/GiantTeam.Organization.Etc.Models";
import { DataResponseResource } from "../../helpers/DataResponseResource";

export class OrganizationDetailsResource extends DataResponseResource<OrganizationDetails> { }
export function createOrganizationDetailsResource(props: { organization: string; }) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization }),
        (props) => postFetchOrganizationDetails(props)
    );
    return new OrganizationDetailsResource(resourceReturn);
}
