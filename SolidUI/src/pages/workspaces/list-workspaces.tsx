import { A, useRouteData } from '@solidjs/router';
import { createResource, Show } from 'solid-js';
import { postFetchRecords } from '../../api/GiantTeam.Data.Api';
import { title, setTitle } from '../../utils/page';
import Table, { TableData } from '../../widgets/Table';

export function WorkspacesPageData() {

  const [resource] = createResource(() => postFetchRecords({
    database: 'info',
    schema: 'public',
    table: 'gt_database',
    columns: null,
    filters: null,
    skip: null,
    take: null,
    verbose: null,
  }));

  return resource;
}

export default function WorkspacePage() {
  setTitle('Workspaces');

  const resource = useRouteData<typeof WorkspacesPageData>();

  const ok = () => resource()?.ok == true;
  const message = () => resource()?.message ?? '';
  const data = (): TableData => {
    const response = resource();
    const data = response?.ok ? response.data : undefined;
    return {
      columns: data?.columns.map(c => c.name) ?? [],
      records: data?.records ?? [],
    };
  };

  return (
    <section class='card md:w-md md:mx-auto'>

      <h1>{title()}</h1>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          {message()}
        </p>
      </Show>

      <Show when={ok()}>
        <Table data={data()} rowLeader={record => <A href={'/workspace/' + record[0]}>View</A>} />
      </Show>

    </section>
  );
}
