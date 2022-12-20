import { RouteDataFuncArgs } from '@solidjs/router';
import { createResource } from 'solid-js';
import { FetchWorkspaceOutput } from '../../api/GiantTeam';
import { postFetchWorkspace } from '../../api/GiantTeam.Data.Api';
import { DataResponse } from '../../utils/httpHelpers';

export interface WorkspacePageModel extends DataResponse<FetchWorkspaceOutput> { }

export const fetchWorkspace = async (workspaceName: string) => {
    const output = await postFetchWorkspace({
        workspaceName
    });
    return output;
};

export const WorkspacePageData = ({ params }: RouteDataFuncArgs) => {
    const [workspace] = createResource(() => params.workspace, fetchWorkspace);
    return workspace;
};

export default WorkspacePageData;