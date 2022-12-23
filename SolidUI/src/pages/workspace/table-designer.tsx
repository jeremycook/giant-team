import { useParams, useSearchParams } from "@solidjs/router";
import { createEffect, createResource, Show } from "solid-js";
import { createMutable, unwrap } from "solid-js/store";
import { Table } from "../../api/GiantTeam";
import { postAlterTable, postFetchWorkspace } from "../../api/GiantTeam.Data.Api";
import { SaveEditFilledIcon } from "../../helpers/icons";
import { setTitle } from "../../utils/page"
import { TableDesignerWidget } from "../../widgets/TableDesigner";

export default function CreateTablePage() {
    setTitle('Create a Table');

    const params = useParams();
    const [search] = useSearchParams();

    const info = {
        workspace: params.workspace as string,
        schema: search.schema as string,
        table: search.table as string,
    };

    const model = createMutable<{ table: Table | null }>({
        table: null,
    });

    const [resource] = createResource(async () => await postFetchWorkspace({
        workspaceName: info.workspace
    }));

    createEffect(() => {
        if (resource()?.ok) {
            const table = resource()?.data
                ?.schemas.find(s => s.name === info.schema)
                ?.tables.find(t => t.name === info.table);
            if (typeof table !== 'undefined') {
                model.table = table;
            }
        }
    })

    const onsubmitform = async (e: SubmitEvent) => {
        e.preventDefault();

        const input = {
            databaseName: info.workspace,
            schemaName: info.schema,
            table: unwrap(model.table!),
        };

        const response = await postAlterTable(input);

        if (response.ok) {
            alert('Table updated!');
        }
        else {
            alert('Error: ' + response.message);
        }
    };

    return (
        <section class='pxy md:w-900px max-w-100% md:mx-auto'>

            <h1>Table Designer</h1>

            <Show when={model.table}>
                <form onsubmit={onsubmitform}>

                    <div class='flex mb rounded paint-gray-100 children:text-lg'>
                        <button class='button'>
                            <SaveEditFilledIcon />
                            Save
                        </button>
                    </div>

                    <div class="pxy b rounded">
                        <TableDesignerWidget table={model.table!} />
                    </div>

                </form>
            </Show>

        </section>
    )
}