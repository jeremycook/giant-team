import { postCreateWorkspace } from '../../api/GiantTeam.Data.Api';
import { go, PageSettings } from '../../partials/Nav';
import { toast } from '../../partials/Notifications';
import { isAuthenticated } from '../../utils/session';
import { FieldSetOptions, FieldStack } from '../../widgets/FieldStack';
import { createMutable } from 'solid-js/store';

const dataOptions: FieldSetOptions = {
  name: { type: 'text', label: 'Organization Name', required: true },
  isPublic: { type: 'boolean', label: 'Public' },
};

export const pageSettings: PageSettings = {
  name: 'New Organization',
  showInNav: () => isAuthenticated(),
}

export default function NewOrganizationPage() {
  const data = createMutable({
    name: '',
    isPublic: false,
  });

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const response = await postCreateWorkspace({
      workspaceName: data.name,
      isPublic: data.isPublic,
    });

    if (response.ok) {
      toast.info('Organization created!');
      go('/org/' + data.name);
    }
    else {
      toast.error(response.message);
    }
  };

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>New Organization</h1>

      <form onSubmit={formSubmit} class='form-grid'>

        <FieldStack data={data} options={dataOptions} />

        <div />
        <div>
          <button class='button button-primary'>
            Create Organization
          </button>
        </div>

      </form>

    </section>
  );
}
