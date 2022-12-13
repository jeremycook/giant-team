import { A, Outlet, useRouteData } from '@solidjs/router';
import { createEffect, For, Resource, Show } from 'solid-js';
import { title, titleSetter } from '../../title';
import { InfoIcon } from '../../utils/icons';
import { WorkspacePageModel } from './workspace.data';

export default function WorkspacePage() {
  titleSetter('Workspace');

  const model = useRouteData<() => Resource<WorkspacePageModel>>();

  const ok = () => model()?.ok == true;
  const message = () => model()?.message || null;
  const data = () => model()?.data;

  createEffect(() => titleSetter(model()?.data?.workspaceName ?? 'Workspace'));

  return (
    <section>

      <h1 class='sr-only'>{title()}</h1>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          {message}
        </p>
      </Show>

      <Show when={ok()}>

        <div class='flex mb'>
          <A class='button rounded-0' href={'./import-data'}>Import Data</A>
          <A class='button rounded-0' href={'./create-schema'}>New Schema</A>
          <A class='button rounded-0' href={'./create-table'}>New Table</A>
          <A class='button rounded-0' href={'./create-view'}>New View</A>
        </div>

        <div class='md:flex'>

          <div class='md:w-200px'>
            <For each={data()!.schemas}>{(schema =>
              <>
                {console.debug(schema.tables)}
                <div class='md:flex flex-col mb'>
                  <strong>{schema.name} <InfoIcon title={'Owned by ' + schema.owner} /></strong>
                  <For each={schema.tables}>{(table =>
                    <A href={`/workspace/${data()!.workspaceName}/schemas/${schema.name}/tables/${table.name}`}>{table.name}</A>
                  )}</For>
                </div>
              </>
            )}</For>

            <div>
              <span>Owner: {data()!.workspaceOwner}</span>
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
