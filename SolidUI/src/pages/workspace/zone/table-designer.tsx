import { A, useParams, useSearchParams } from "@solidjs/router";
import { createEffect, createResource } from "solid-js";
import { createMutable, unwrap } from "solid-js/store";
import { postFetchWorkspace, postAlterTable } from "../../../bindings/GiantTeam.Data.Api.Controllers";
import { Table } from "../../../bindings/GiantTeam.DatabaseDefinition.Models";
import { AlterTableInput } from "../../../bindings/GiantTeam.Workspaces.Services";
import { SaveEditFilledIcon } from "../../../helpers/icons";
import { createUrl } from "../../../helpers/urlHelpers";
import { setTitle } from "../../../utils/page"
import { TableDesignerWidget } from "../../../widgets/TableDesigner";

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
        const response = resource();
        const data = response?.ok ? response.data : undefined;
        if (data) {
            const table = data
                ?.zones.find(z => z.name === info.schema)
                ?.tables.find(t => t.name === info.table);
            if (typeof table !== 'undefined') {
                model.table = table;
            }
        }
    })

    const onsubmitform = async (e: SubmitEvent) => {
        e.preventDefault();

        const input: AlterTableInput = {
            databaseName: info.workspace,
            schemaName: info.schema,
            tableName: info.table,
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

            {model.table ?
                <form onsubmit={onsubmitform}>

                    <div class='flex gap-1 mb rounded paint-gray-100'>
                        <button class='button paint-primary'>
                            <SaveEditFilledIcon />
                            Save
                        </button>
                        <A class='button' href={createUrl('../table', { schema: info.schema, table: info.table })}>Open Table</A>
                    </div>

                    <div class="pxy b rounded">
                        <TableDesignerWidget
                            table={model.table}
                            lockedColumnNames={model.table.columns.map(c => c.name)} />
                    </div>

                </form>
                :
                null
            }

        </section>
    )
}