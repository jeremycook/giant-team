import { A, useRouteData } from '@solidjs/router';
import { createResource, Show } from 'solid-js';
import { postFetchRecords } from '../../api/GiantTeam.Data.Api';
import { title, setTitle } from '../../utils/title';
import Table, { TableData } from '../../widgets/Table';

export function WorkspacesPageData() {

  const [resource] = createResource(() => postFetchRecords({
    database: 'info',
    schema: 'public',
    table: 'gt_database',
  }));

  return resource;
}

export default function WorkspacePage() {
  setTitle('Workspaces');

  const model = useRouteData<typeof WorkspacesPageData>();

  const ok = () => model()?.ok == true;
  const message = () => model()?.message ?? '';
  const data = (): TableData => ({
    columns: model()?.data?.columns.map(c => c.name) ?? [],
    records: model()?.data?.records ?? []
  });

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
