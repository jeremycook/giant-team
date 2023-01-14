import { createResource } from "solid-js";
import { postFetchInode } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { FetchInodeResult } from "../../bindings/GiantTeam.Organization.Services";
import { DataResponseResource } from "../../helpers/DataResponseResource";


export class InodeResource extends DataResponseResource<FetchInodeResult> { }
export function createInodeResource(props: { organization: string; path: string; }) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization, path: props.path }),
        (props) => postFetchInode(props)
    );
    return new InodeResource(resourceReturn);
}
