import { useParams } from "@solidjs/router";
import { createResource } from "solid-js";
import { FetchWorkspaceOutput } from "../../api/GiantTeam";
import { postFetchWorkspace } from "../../api/GiantTeam.Data.Api";
import { DataResponse } from "../../utils/httpHelpers";

export interface WorkspacePageModel extends DataResponse<FetchWorkspaceOutput> {
}

export const fetchWorkspace = async (workspaceName: string) => {
    useParams();
    const output = await postFetchWorkspace({
        workspaceName
    });

    return output;
};

export default function ({ params }: { params: { id: string } }) {
    const [workspace] = createResource(() => params.id, fetchWorkspace);
    return workspace;
};