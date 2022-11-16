import { createSignal, Show } from 'solid-js';
import { CreateWorkspaceStatus, postCreateWorkspace } from '../api/GiantTeam.Data.Api';
import { createId } from '../utils/elementHelpers';
import { createUrl } from '../utils/urlHelpers';

export default function CreateWorkspace() {

  // Input
  const [workspaceName, workspaceNameSetter] = createSignal("");

  // Output
  const [errorMessage, errorMessageSetter] = createSignal("");
  const [successMessage, successMessageSetter] = createSignal("");

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const output = await postCreateWorkspace({
      workspaceName: workspaceName()
    });

    console.log(output);

    switch (output.status) {
      case CreateWorkspaceStatus.Success:
        errorMessageSetter("");
        successMessageSetter("Workspace created! Taking you to it now…");
        location.assign(createUrl("/workspace", { workspaceId: output.workspaceId }));
        break;
      case CreateWorkspaceStatus.Problem:
        errorMessageSetter(output.message);
        successMessageSetter("");
        break;
      default:
        throw Error(`Unsupported CreateWorkspaceStatus ${output.status}.`);
    }
  };

  return (
    <section>

      <h1>Create Workspace</h1>

      <Show when={errorMessage()}>
        <p class="text-red">
          {errorMessage()}
        </p>
      </Show>

      <Show when={successMessage()}>
        <p class="text-green">
          {successMessage()}
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
