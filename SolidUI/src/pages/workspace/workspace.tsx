import { A, useRouteData } from '@solidjs/router';
import { createEffect, For, Resource, Show } from 'solid-js';
import { authorize } from '../../session';
import { title, titleSetter } from '../../title';
import { InfoIcon } from '../../utils/icons';
import { WorkspacePageModel } from './workspace.data';

export default function WorkspacePage() {
  authorize();
  titleSetter('Workspace');

  const model = useRouteData<() => Resource<WorkspacePageModel>>();

  const ok = () => model()?.ok == true;
  const message = () => model()?.message || null;
  const data = () => model()?.data;

  createEffect(() => titleSetter(model()?.data?.workspaceName ?? 'Workspace'));

  return (
    <section class='card'>

      <h1>{title()}</h1>

      <Show when={model()}>

        <Show when={message}>
          <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
            {message}
          </p>
        </Show>

        <Show when={ok() && data()}>
          <p>
            Owner: {data()!.workspaceOwner}
          </p>

          <div>
            <div class='menu'>
              <A href={'./import-data'}>Import Data</A>
              <A href={'./create-table'}>New Table</A>
              <A href={'./create-view'}>New View</A>
            </div>

            <div class='menu'>
              <For each={data()!.schemas}>{(schema =>
                <>
                  <strong>{schema.name} <InfoIcon title={'Owned by ' + schema.owner} /></strong>
                  <For each={schema.tables}>{(table =>
                    <A href={`./schemas/${schema.name}/tables/${table.name}`}>{table.name}</A>
                  )}</For>
                </>
              )}</For>
            </div>
          </div>

        </Show>

      </Show>

    </section>
  );
}
