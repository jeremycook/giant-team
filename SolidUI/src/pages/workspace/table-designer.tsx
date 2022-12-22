import { useParams, useSearchParams } from "@solidjs/router";
import { createEffect, createResource, Show } from "solid-js";
import { createMutable, unwrap } from "solid-js/store";
import { Table } from "../../api/GiantTeam";
import { postAlterTable, postFetchWorkspace } from "../../api/GiantTeam.Data.Api";
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
        table: null
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
                console.log(table);
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
        <section class='card card md:w-900px max-w-100% md:mx-auto'>

            <h1>Table Designer</h1>

            <Show when={model.table}>
                <form onsubmit={onsubmitform}>

                    <TableDesignerWidget table={model.table!} />

                    <div>
                        <button class='button'>Apply Changes</button>
                    </div>

                </form>
            </Show>

        </section>
    )
}