import { createSignal, Show } from 'solid-js';
import { createId } from '../../helpers/htmlHelpers';
import { postCreateWorkspace } from '../../api/GiantTeam.Data.Api';
import { setTitle } from '../../utils/page';
import { useNavigate } from '@solidjs/router';
import { Breadcrumb } from '../../utils/nav';

export default function CreateWorkspacePage() {
  setTitle('Create a Workspace');

  const navigate = useNavigate();

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal('');

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;

    const workspaceName = form.workspaceName.value;

    const output = await postCreateWorkspace({
      workspaceName: workspaceName,
      isPublic: false,
    });

    okSetter(output.ok);

    if (output.ok) {
      messageSetter('Workspace created! Taking you to it now…');
      navigate('/workspace/' + workspaceName);
    }
    else {
      messageSetter(output.message);
    }
  };

  return (
    <section class='card md:w-md md:mx-auto'>

      <Breadcrumb link={{ text: 'New Workspace', href: '/workspaces/new-workspace' }} />

      <h1>New Workspace</h1>

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
          autocomplete='no'
          use:autofocus
        />

        <div />
        <div>
          <button class='button'>
            Create Workspace
          </button>
        </div>

      </form>

    </section>
  );
}
