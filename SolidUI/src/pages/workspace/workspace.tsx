import { A, Outlet, RouteDataFuncArgs, useRouteData } from '@solidjs/router';
import { createEffect, createResource, For, Show } from 'solid-js';
import { postFetchWorkspace } from '../../api/GiantTeam.Data.Api';
import { title, setTitle } from '../../utils/title';
import { createUrl } from '../../helpers/urlHelpers';

const fetchWorkspace = async (workspaceName: string) => {
  const output = await postFetchWorkspace({
      workspaceName
  });
  return output;
};

export const WorkspacePageData = ({ params }: RouteDataFuncArgs) => {
  const [workspace] = createResource(() => params.workspace, fetchWorkspace);
  return workspace;
};

export default function WorkspacePage() {
  setTitle('Workspace');

  const model = useRouteData<typeof WorkspacePageData>();

  const ok = () => model()?.ok == true;
  const message = () => model()?.message || null;
  const data = () => model()?.data;

  createEffect(() => setTitle(data()?.name ?? 'Workspace'));

  return (
    <section>

      <h1 class='sr-only'>{title()}</h1>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          {message}
        </p>
      </Show>

      <Show when={ok()}>

        <div class='md:flex'>

          <div class='md:min-w-200px md:p1'>

            <div class='flex flex-col mb'>
              <A class='button p-1 rounded-0' href={'./import-data'}>Import Data</A>
              <A class='button p-1 rounded-0' href={'./create-schema'}>Add Schema</A>
              <A class='button p-1 rounded-0' href={'./create-table'}>Add Table</A>
              <A class='button p-1 rounded-0' href={'./create-view'}>Add View</A>
            </div>

            <For each={data()!.schemas}>{(schema =>
              <>
                <div class='md:flex flex-col mb'>
                  <div>
                    <strong>{schema.name}</strong>
                    <A href={`schemas/${schema.name}/edit`}>Edit</A>
                  </div>
                  <For each={schema.tables}>{(table =>
                    <A href={createUrl('table', { schema: schema.name, table: table.name })} class='is-active:font-bold'>{table.name}</A>
                  )}</For>
                </div>
              </>
            )}</For>

            <div>
              <span>Owner: {data()!.owner}</span>
            </div>
          </div>

          <div class='flex-grow'>
            <Outlet />
          </div>

        </div>

      </Show>

    </section >
  );
}
