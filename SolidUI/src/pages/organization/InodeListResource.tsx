import { createResource } from "solid-js";
import { postFetchInodeList } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode } from "../../bindings/GiantTeam.Organization.Etc.Models";
import { DataResponseResource } from "../../helpers/DataResponseResource";

export class InodeListResource extends DataResponseResource<Inode[]> { }
export function createInodeListResource(props: { organization: string; path: string }) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization, path: props.path }),
        (props) => postFetchInodeList(props)
    );
    return new InodeListResource(resourceReturn);
}
