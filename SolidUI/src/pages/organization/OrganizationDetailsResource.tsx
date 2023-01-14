import { createResource } from "solid-js";
import { postFetchOrganizationDetails } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { FetchOrganizationDetailsResult } from "../../bindings/GiantTeam.Organization.Services";
import { DataResponseResource } from "../../helpers/DataResponseResource";


export class OrganizationDetailsResource extends DataResponseResource<FetchOrganizationDetailsResult> { }
export function createOrganizationDetailsResource(props: { organization: string; }) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization }),
        (props) => postFetchOrganizationDetails(props)
    );
    return new OrganizationDetailsResource(resourceReturn);
}
