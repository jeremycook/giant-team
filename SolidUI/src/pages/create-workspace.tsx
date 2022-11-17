import { createSignal, Show } from 'solid-js';
import { CreateWorkspaceStatus } from '../api/GiantTeam';
import { postCreateWorkspace } from '../api/GiantTeam.Data.Api';
import { createId } from '../utils/elementHelpers';
import { createUrl } from '../utils/urlHelpers';

export default function CreateWorkspace() {

  // Input
  const [workspaceName, workspaceNameSetter] = createSignal("");

  // Output
  const [status, statusSetter] = createSignal<CreateWorkspaceStatus>(null);
  const [message, messageSetter] = createSignal("");

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const output = await postCreateWorkspace({
      workspaceName: workspaceName()
    });

    console.log(output);

    statusSetter(output.status);
    switch (output.status) {
      case CreateWorkspaceStatus.Success:
        messageSetter("Workspace created! Taking you to it now…");
        location.assign(createUrl("/workspace", { workspaceId: output.workspaceId }));
        break;
      case CreateWorkspaceStatus.Problem:
        messageSetter(output.message);
        break;
      default:
        throw Error(`Unsupported CreateWorkspaceStatus ${output.status}.`);
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
            value={workspaceName()}
            onChange={e => workspaceNameSetter(e.currentTarget.value)}
            required
            autofocus
          />
        </div>

        <button type="submit">
          Create Workspace
        </button>

      </form>

    </section>
  );
}
