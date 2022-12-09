import { createSignal, Show } from 'solid-js';
import { createId } from '../utils/htmlHelpers';
import { createUrl } from '../utils/urlHelpers';
import { postCreateWorkspace } from '../api/GiantTeam.Data.Api';
import { authorize } from '../session';
import { titleSetter } from '../title';

export default function CreateWorkspace() {
  titleSetter("Create a Workspace");
  authorize()

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
      location.assign(createUrl("/workspace", { workspace_name: output.data!.workspaceName }));
    }
    else {
      messageSetter(output.message);
    }
  };

  return (
    <section class="card md:w-md">

      <h1>Create a Workspace</h1>

      <Show when={message()}>
        <p class={(ok() ? "text-ok" : "text-error")} role="alert">
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit} class="form-grid">

        <label for={createId("workspaceName")}>
          Workspace Name
        </label>
        <input
          id={createId("workspaceName")}
          name="workspaceName"
          required
          autofocus
        />

        <label for={createId("workspaceOwner")}>
          Workspace Owner
        </label>
        <input
          id={createId("workspaceOwner")}
          name="workspaceOwner"
          required
        />

        <div />
        <div>
          <button type="submit" class="button">
            Create Workspace
          </button>
        </div>

      </form>

    </section>
  );
}
