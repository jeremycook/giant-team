import { Outlet, RouteDataFuncArgs, useParams, useRouteData } from '@solidjs/router';
import { createResource, Show } from 'solid-js';
import { postFetchWorkspace } from '../../bindings/GiantTeam.Data.Api.Controllers';
import { ObjectStatusResponse } from '../../helpers/httpHelpers';
import { combinePaths } from '../../helpers/urlHelpers';
import { Breadcrumb } from '../../utils/nav';
import StatusCodePage from '../status-code';

const fetchWorkspace = async ({ workspaceName }: { workspaceName: string }) => {
  const output = await postFetchWorkspace({
    workspaceName
  });
  return output;
};

export const WorkspaceLayoutData = ({ params }: RouteDataFuncArgs) => {
  const [workspace] = createResource(() => ({ workspaceName: params.workspace }), fetchWorkspace);
  return workspace;
};

export const useWorkspaceParams = () => {
  const params = useParams();
  return {
    workspace: params.workspace
  };
};

const workspacePattern = new RegExp('^/workspace/([^/]+)', 'i');

export function createWorkspaceUrl(...paths: string[]) {
  const workspaceName = workspacePattern.exec(location.pathname)![1];
  return combinePaths('/workspace', workspaceName, ...paths);
}

export const useWorkspaceRouteData = () => {
  return useRouteData<typeof WorkspaceLayoutData>();
}

export default function WorkspaceLayout() {
  const workspaceRouteData = useWorkspaceRouteData();

  const workspace = () => {
    const ws = workspaceRouteData();
    return ws?.ok ? ws.data : undefined!;
  };

  return (<>
    {workspaceRouteData()?.ok === false ?

      <StatusCodePage {...workspaceRouteData() as ObjectStatusResponse} />

      :

      <Show when={workspace()}>
        <Breadcrumb link={{ text: workspace().name, href: createWorkspaceUrl() }} />
        <Outlet />
      </Show>
    }
  </>);
}
