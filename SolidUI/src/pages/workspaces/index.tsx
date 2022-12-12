import { A } from '@solidjs/router';
import { createSignal, Show } from 'solid-js';
import { FetchRecordsOutput } from '../../api/GiantTeam';
import { postFetchRecords } from '../../api/GiantTeam.Data.Api';
import { authorize } from '../../session';
import { title, titleSetter } from '../../title';
import Table from '../../widgets/Table';

export default function WorkspacePage() {
  authorize();
  titleSetter("Workspaces");

  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal("");
  const [data, dataSetter] = createSignal<FetchRecordsOutput | null>();

  const fetchWorkspaces = async () => {

    const output = await postFetchRecords({
      database: 'info',
      schema: 'public',
      table: 'gt_database',
      orderBy: [
        { column: 'name' }
      ]
    });

    okSetter(output.ok);
    messageSetter(output.message);
    dataSetter(output.data);
  };

  fetchWorkspaces();

  return (
    <section class="card md:w-md">

      <h1>{title()}</h1>

      <Show when={message()}>
        <p class={(ok() ? "text-ok" : "text-error")} role="alert">
          {message()}
        </p>
      </Show>

      <Show when={ok() && data()?.records}>
        <Table data={data()!} rowLeader={record => <A href={'/workspace/' + record[0] }>View</A>} />
      </Show>

    </section>
  );
}