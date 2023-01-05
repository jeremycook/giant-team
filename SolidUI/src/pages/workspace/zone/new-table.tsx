import { useParams, useSearchParams } from "@solidjs/router";
import { createMutable, unwrap } from "solid-js/store";
import { postCreateTable } from "../../../api/GiantTeam.Data.Api.Controllers";
import { Table, StoreType, TableIndexType } from "../../../api/GiantTeam.DatabaseDefinition.Models";
import { CreateTableInput } from "../../../api/GiantTeam.Workspaces.Services";
import { SaveEditFilledIcon } from "../../../helpers/icons";
import { Breadcrumb } from "../../../utils/nav";
import { TableDesignerWidget } from "../../../widgets/TableDesigner";
import { createZoneUrl } from "./zone-layout";

export default function CreateTablePage() {
    const params = useParams();
    const [search] = useSearchParams();

    const info = {
        workspace: params.workspace as string,
        schema: search.schema as string,
    };

    let nextPosition = 1;
    const table = createMutable<Table>({
        name: '',
        owner: '',
        columns: [{
            position: nextPosition++,
            name: 'id',
            storeType: StoreType.uuid,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: 'gen_random_uuid()',
        }],
        indexes: [{
            name: '',
            indexType: TableIndexType.PrimaryKey,
            columns: ['id'],
        }],
    });

    const onsubmitform = async (e: SubmitEvent) => {
        e.preventDefault();

        const value = unwrap(table);

        const input: CreateTableInput = {
            databaseName: info.workspace,
            schemaName: info.schema,
            tableName: table.name,
            columns: value.columns,
            indexes: value.indexes,
        };

        const response = await postCreateTable(input);

        if (response.ok) {
            alert('Table created!');
        }
        else {
            alert('Error: ' + response.message);
        }
    };

    return (<>
        <Breadcrumb link={{ text: 'New Table', href: createZoneUrl('new-table') }} />

        <section class='pxy md:w-900px max-w-100% md:mx-auto'>

            <h1>New Table</h1>

            <form onsubmit={onsubmitform}>

                <div class='flex gap-1 mb rounded paint-gray-100'>
                    <button class='button paint-primary'>
                        <SaveEditFilledIcon />
                        Save
                    </button>
                    <button type='button' class='button'
                        onclick={e => {
                            table.columns = [...table.columns,
                            {
                                position: nextPosition++,
                                name: 'title',
                                storeType: StoreType.text,
                                isNullable: false,
                                computedColumnSql: '',
                                defaultValueSql: '',
                            },
                            {
                                position: nextPosition++,
                                name: 'body',
                                storeType: StoreType.text,
                                isNullable: false,
                                computedColumnSql: '',
                                defaultValueSql: '',
                            },
                            {
                                position: nextPosition++,
                                name: 'slug',
                                storeType: StoreType.text,
                                isNullable: false,
                                computedColumnSql: `trim(regexp_replace(title, '[^\w]+', '-', 'g'), '-')`,
                                defaultValueSql: '',
                            },
                            {
                                position: nextPosition++,
                                name: 'created',
                                storeType: StoreType.timestampTz,
                                isNullable: false,
                                computedColumnSql: '',
                                defaultValueSql: `(CURRENT_TIMESTAMP AT TIME ZONE 'UTC')`,
                            },
                            {
                                position: nextPosition++,
                                name: 'created_by',
                                storeType: StoreType.text, // TODO: uuid once default value becomes meta.current_user_id()
                                isNullable: false,
                                computedColumnSql: '',
                                defaultValueSql: 'CURRENT_USER', // TODO: meta.current_user_id()
                            }];
                            table.indexes = [...table.indexes, {
                                name: '',
                                indexType: TableIndexType.UniqueConstraint,
                                columns: ['slug'],
                            }];
                        }}>
                        Add Example
                    </button>
                </div>

                <div class="pxy b rounded">
                    <TableDesignerWidget table={table} />
                </div>

            </form>

        </section>
    </>);
}