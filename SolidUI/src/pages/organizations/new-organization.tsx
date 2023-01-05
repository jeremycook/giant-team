import { go, PageSettings } from '../../partials/Nav';
import { toast } from '../../partials/Alerts';
import { isAuthenticated } from '../../utils/session';
import { FieldSetOptions, FieldStack } from '../../widgets/FieldStack';
import { createMutable } from 'solid-js/store';
import { createEffect } from 'solid-js';
import { postCreateOrganization } from '../../bindings/GiantTeam.Data.Api.Controllers';

export const pageSettings: PageSettings = {
  name: 'New Organization',
  showInNav: () => isAuthenticated(),
}

export default function NewOrganizationPage() {
  const data = createMutable({
    name: '',
    databaseName: '',
    taintedDatabaseName: false,
  });

  const dataOptions: FieldSetOptions = {
    name: { type: 'text', label: 'Organization Name', required: true },
    databaseName: {
      type: 'text', label: 'Database Name', required: true, maxLength: 50, pattern: '^[a-z][a-z0-9_]*$',
      title: 'This must start with a lowercase letter, and may be followed by lowercase letters, numbers or the underscore',
      onfocus: () => data.taintedDatabaseName = true
    },
  };

  createEffect(() => {
    if (!data.taintedDatabaseName)
      data.databaseName = data.name.toLowerCase().replaceAll(/[^a-z0-9_]+/g, '_').replace(/^[^a-z]+/, '').replace(/[_]+$/, '');
  });

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const response = await postCreateOrganization({
      name: data.name,
      databaseName: data.databaseName,
    });

    if (response.ok) {
      toast.info('Organization created!');
      go('/organizations/' + response.data.organizationId);
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
