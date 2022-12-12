import { createSignal, Show } from 'solid-js';
import { createId } from '../../utils/htmlHelpers';
import { postCreateWorkspace } from '../../api/GiantTeam.Data.Api';
import { authorize, session } from '../../session';
import { titleSetter } from '../../title';

export default function CreateWorkspacePage() {
  authorize()
  titleSetter('Create a Workspace');

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal('');

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;

    const output = await postCreateWorkspace({
      workspaceName: form.workspaceName.value,
      workspaceOwner: form.workspaceOwner.value,
    });

    okSetter(output.ok);

    if (output.ok) {
      messageSetter('Workspace created! Taking you to it now…');
      location.assign('/workspace/' + output.data!.workspaceName);
    }
    else {
      messageSetter(output.message);
    }
  };

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>Create a Workspace</h1>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit} class='form-grid'>

        <label for={createId('workspaceName')}>
          Workspace Name
        </label>
        <input
          id={createId('workspaceName')}
          name='workspaceName'
          required
          autofocus
          autocomplete='no'
        />

        <label for={createId('workspaceOwner')}>
          Workspace Owner
        </label>
        <select
          id={createId('workspaceOwner')}
          name='workspaceOwner'
          required
        >
          <option>Choose…</option>
          <option
            value={session().username}
            selected={true}
          >{session().username}</option>
        </select>

        <div />
        <div>
          <button type='submit' class='button'>
            Create Workspace
          </button>
        </div>

      </form>

    </section>
  );
}
