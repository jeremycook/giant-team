import { NavLink, Outlet, RouteDataFuncArgs, useBeforeLeave, useParams, useRouteData } from '@solidjs/router';
import { createEffect, createResource, Show } from 'solid-js';
import { Workspace } from '../../api/GiantTeam';
import { postFetchWorkspace } from '../../api/GiantTeam.Data.Api';
import { ObjectStatusResponse } from '../../helpers/httpHelpers';
import { combinePaths, createUrl } from '../../helpers/urlHelpers';
import { Breadcrumb, useBreadcrumbContext } from '../../utils/nav';
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

export function createWorkspaceUrl() {
  const params = useWorkspaceParams();
  return `/workspace/${params.workspace}`;
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
