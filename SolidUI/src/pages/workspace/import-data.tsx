import { useNavigate, useParams } from '@solidjs/router';
import { createEffect, createSignal, Show } from 'solid-js';
import { setTitle } from '../../utils/page';
import { createId, stringifyBlob } from '../../helpers/htmlHelpers';
import { WarningIcon } from '../../helpers/icons';
import { postImportData } from '../../bindings/GiantTeam.Data.Api.Controllers';

export default function ImportDataPage() {
  setTitle('Import Data');

  const navigate = useNavigate();
  const params = useParams();

  const database = () => params.workspace;
  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal('');

  createEffect(() => setTitle(`Import Data into ${database()}`));

  const formSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    const fileBlob = (form.file as HTMLInputElement).files![0] as Blob;

    const output = await postImportData({
      database: database(),
      schema: form.schema.value,
      table: form.table.value,
      createTableIfNotExists: form.createTableIfNotExists.checked,
      data: await stringifyBlob(fileBlob) as string,
    });

    okSetter(output.ok);
    messageSetter(output.message);

    if (output.ok && output.data) {
      messageSetter('Data imported. Taking you to it…');
      navigate('../schemas/' + output.data.schema + '/tables/' + output.data.table);
    }
  };

  const onChangeFile = (e: any) => {
    const form = e.target.form! as HTMLFormElement;

    if (!form.table.value) {

      const file = form.file?.files?.[0] as File;
      if (file) {
        form.table.value = file.name;
      }

    }
  }

  return (
    <section class='pxy max-w-md'>

      <Show when={message()}>
        <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
          <WarningIcon class='animate-bounce-in' />{' '}
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit} class='form-grid'>

        <label for={createId('file')}>
          CSV File
        </label>
        <input
          id={createId('file')}
          name='file'
          type='file'
          required
          accept='.csv, text/csv'
          onchange={onChangeFile}
        />

        <label for={createId('schema')}>
          Schema
        </label>
        <input
          id={createId('schema')}
          name='schema'
          required
        />

        <label for={createId('table')}>
          Table
        </label>
        <input
          id={createId('table')}
          name='table'
          required
        />

        <div />
        <div>
          <label>
            <input
              name='createTableIfNotExists'
              type='checkbox'
            /> Create table if it doesn't exists
          </label>
        </div>

        <div />
        <div>
          <button type='submit' class='button'>
            Import Data
          </button>
        </div>

      </form>

    </section>
  );
}
