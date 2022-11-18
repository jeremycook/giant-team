import { createSignal, Show } from 'solid-js';
import { FetchWorkspaceInput, FetchWorkspaceOutput } from '../api/GiantTeam';
import { postFetchWorkspace } from '../api/GiantTeam.Data.Api';
import { HttpStatusCode, postJson } from '../utils/fetchHelpers';
import { getParam } from '../utils/urlHelpers';

export default function WorkspacePage() {

  // Input

  // Output
  const [status, statusSetter] = createSignal<number>(null);
  const [message, messageSetter] = createSignal("");
  const [workspace, workspaceSetter] = createSignal<FetchWorkspaceOutput>();

  const fetchWorkspace = async () => {

    const workspaceName = getParam("workspaceName");

    const output = await postJson<FetchWorkspaceInput, FetchWorkspaceOutput>(
      "/api/fetch-workspace",
      {
        workspaceName: workspaceName
      });

    statusSetter(output.status);
    messageSetter(output.message ?? "");
    workspaceSetter(output.data ?? null);
  };

  fetchWorkspace();

  return (
    <section>

      <h1>Workspace</h1>

      <Show when={message()}>
        <p class={(status() == HttpStatusCode.Ok ? "text-green" : "text-red")}>
          {message()}
        </p>
      </Show>

      <Show when={typeof workspace() === "object"}>
        <pre>{JSON.stringify(workspace())}</pre>
      </Show>

    </section>
  );
}
