import { useLocation } from '@solidjs/router';
import { createSignal, Show } from 'solid-js';
import { FetchWorkspaceOutput } from '../api/GiantTeam';
import { postFetchWorkspace } from '../api/GiantTeam.Data.Api';
import { authorize } from '../session';
import { titleSetter } from '../title';

export default function WorkspacePage() {
  titleSetter("Logout");
  authorize();

  const location = useLocation();

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal("");
  const [data, dataSetter] = createSignal<FetchWorkspaceOutput | null>();

  const fetchWorkspace = async (workspaceName: string) => {

    const output = await postFetchWorkspace({
      workspaceName
    });

    okSetter(output.ok);
    messageSetter(output.message);
    dataSetter(output.data);
  };

  fetchWorkspace(location.query.workspace_name);

  return (
    <section class="card md:w-md">

      <h1>Workspace</h1>

      <Show when={message()}>
        <p class={(ok() ? "text-green" : "text-red")}>
          {message()}
        </p>
      </Show>

      <Show when={ok()}>
        <pre>{JSON.stringify(data())}</pre>
      </Show>

    </section>
  );
}
