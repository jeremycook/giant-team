import { createResource } from "solid-js";
import { postFetchInodeByPath } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode } from "../../bindings/GiantTeam.Organization.Etc.Models";
import { DataResponseResource } from "../../helpers/DataResponseResource";

export class InodeResource extends DataResponseResource<Inode> { }
export function createInodeResource(props: { organization: string; path: string; }) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization, path: props.path }),
        (props) => postFetchInodeByPath(props)
    );
    return new InodeResource(resourceReturn);
}
