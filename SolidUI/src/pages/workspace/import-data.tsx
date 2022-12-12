import { useNavigate, useParams } from '@solidjs/router';
import { createEffect, createSignal, Show } from 'solid-js';
import { postImportData } from '../../api/GiantTeam.Data.Api';
import { authorize } from '../../session';
import { title, titleSetter } from '../../title';
import { createId, stringifyBlob } from '../../utils/htmlHelpers';
import { WarningIcon } from '../../utils/icons';

export default function ImportDataPage() {
  authorize();
  titleSetter("Import Data");

  const navigate = useNavigate();
  const params = useParams();

  const database = () => params.workspace;
  const [ok, okSetter] = createSignal(true);
  const [message, messageSetter] = createSignal("");

  createEffect(() => titleSetter(`Import Data into ${database()}`));

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
      messageSetter("Data imported. Taking you to it…");
      navigate('./schema/' + output.data.schema + '/table/' + output.data.table);
    }
  };

  return (
    <section class="card md:w-md md:mx-auto">

      <h1>{title}</h1>

      <Show when={message()}>
        <p class={(ok() ? "text-ok" : "text-error")} role="alert">
          <WarningIcon class="animate-bounce-in" />{' '}
          {message()}
        </p>
      </Show>

      <form onSubmit={formSubmit} class="form-grid">

        <label for={createId("schema")}>
          Schema
        </label>
        <input
          id={createId("schema")}
          name="schema"
          required
        />

        <label for={createId("table")}>
          Table
        </label>
        <input
          id={createId("table")}
          name="table"
          required
        />

        <div />
        <label><input
          name="createTableIfNotExists"
          type="checkbox"
        /> Create table if it doesn't exists</label>

        <label for={createId("file")}>
          CSV File
        </label>
        <input
          id={createId("file")}
          name="file"
          type="file"
          required
          accept=".csv, text/csv"
        />

        <div />
        <div>
          <button type="submit" class="button">
            Import Data
          </button>
        </div>

      </form>

    </section>
  );
}
