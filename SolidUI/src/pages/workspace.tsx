import { createSignal, Show } from 'solid-js';
import { FetchWorkspaceInput, FetchWorkspaceOutput } from '../api/GiantTeam';
import { postFetchWorkspace } from '../api/GiantTeam.Data.Api';
import { postJson } from '../utils/httpHelpers';
import { getParam } from '../utils/urlHelpers';

export default function WorkspacePage() {

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal("");
  const [data, dataSetter] = createSignal<FetchWorkspaceOutput>();

  const fetchWorkspace = async () => {

    const workspaceName = getParam("workspaceName");

    const output = await postFetchWorkspace({
      workspaceName: workspaceName
    });

    okSetter(output.ok);
    messageSetter(output.message);
    dataSetter(output.data);
  };

  fetchWorkspace();

  return (
    <section>

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
