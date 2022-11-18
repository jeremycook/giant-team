import { createSignal, from, Show } from 'solid-js';
import { CreateWorkspaceInput, CreateWorkspaceStatus } from '../api/GiantTeam';
import { postCreateWorkspace } from '../api/GiantTeam.Data.Api';
import { createId } from '../utils/htmlHelpers';
import { createUrl } from '../utils/urlHelpers';

export default function CreateWorkspace() {

  // Output
  const [status, statusSetter] = createSignal<CreateWorkspaceStatus>(null);
  const [message, messageSetter] = createSignal("");

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;

    const output = await postCreateWorkspace({
      workspaceName: form.workspaceName.value,
      workspaceOwner: form.workspaceOwner.value,
    });

    statusSetter(output.status);
    switch (output.status) {
      case CreateWorkspaceStatus.Success:
        messageSetter("Workspace created! Taking you to it now…");
        location.assign(createUrl("/workspace", { workspaceName: output.workspaceName }));
        break;
      case CreateWorkspaceStatus.Problem:
        messageSetter(output.message);
        break;
      default:
        throw Error(`Unsupported CreateWorkspaceStatus: ${output.status}.`);
    }
  };

  return (
    <section>

      <h1>Create Workspace</h1>

      <Show when={message()}>
        <p class={(status() == CreateWorkspaceStatus.Success ? "text-green" : "text-red")}>
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

        <div>
          <label><input type="checkbox" name="isPublic" /> Public workspace</label>
        </div>

        <button type="submit">
          Create Workspace
        </button>

      </form>

    </section>
  );
}
