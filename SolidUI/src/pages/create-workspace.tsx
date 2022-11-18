import { createSignal, Show } from 'solid-js';
import { CreateWorkspaceInput, CreateWorkspaceOutput } from '../api/GiantTeam';
import { HttpStatusCode, postJson } from '../utils/httpHelpers';
import { createId } from '../utils/htmlHelpers';
import { createUrl } from '../utils/urlHelpers';
import { postCreateWorkspace } from '../api/GiantTeam.Data.Api';

export default function CreateWorkspace() {

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal("");

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;

    const output = await postCreateWorkspace({
      workspaceName: form.workspaceName.value,
      workspaceOwner: form.workspaceOwner.value,
    });

    okSetter(output.ok);

    if (output.ok) {
      messageSetter("Workspace created! Taking you to it now…");
      location.assign(createUrl("/workspace", { workspaceName: output.data.workspaceName }));
    }
    else {
      messageSetter(output.message);
    }
  };

  return (
    <section>

      <h1>Create Workspace</h1>

      <Show when={message()}>
        <p class={(ok() ? "text-green" : "text-red")}>
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit}>

        <div>
          <label for={createId("workspaceName")}>
            Workspace Name
          </label>
          <input
            id={createId("workspaceName")}
            name="workspaceName"
            required
            autofocus
          />
        </div>

        <div>
          <label for={createId("workspaceOwner")}>
            Workspace Owner
          </label>
          <input
            id={createId("workspaceOwner")}
            name="workspaceOwner"
            required
          />
        </div>

        <button type="submit">
          Create Workspace
        </button>

      </form>

    </section>
  );
}
