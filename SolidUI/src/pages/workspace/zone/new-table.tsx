import { useParams, useSearchParams } from "@solidjs/router";
import { createMutable, unwrap } from "solid-js/store";
import { StoreType, Table, TableIndexType } from "../../../api/GiantTeam";
import { postCreateTable } from "../../../api/GiantTeam.Data.Api";
import { SaveEditFilledIcon } from "../../../helpers/icons";
import { combinePaths } from "../../../helpers/urlHelpers";
import { Breadcrumb, BreadcrumbTrail } from "../../../utils/nav";
import { setTitle } from "../../../utils/page"
import { TableDesignerWidget } from "../../../widgets/TableDesigner";
import { createZoneUrl, useZoneData } from "./zone-layout";

export default function CreateTablePage() {
    const zoneData = useZoneData();

    // setTitle('Create a Table');

    const params = useParams();
    const [search] = useSearchParams();

    const info = {
        workspace: params.workspace as string,
        schema: search.schema as string,
    };

    let tmpPosition = 1;
    const table = createMutable<Table>({
        name: 'Blog Post',
        owner: '',
        columns: [{
            position: tmpPosition++,
            name: 'Id',
            storeType: StoreType.uuid,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: 'gen_random_uuid()',
        },
        {
            position: tmpPosition++,
            name: 'Title',
            storeType: StoreType.text,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: '',
        },
        {
            position: tmpPosition++,
            name: 'Body',
            storeType: StoreType.text, // TODO: 'html'
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: '',
        },
        {
            position: tmpPosition++,
            name: 'Slug',
            storeType: StoreType.text,
            isNullable: false,
            computedColumnSql: `trim(regexp_replace("Title", '[^\w]+', '-', 'g'), '-')`,
            defaultValueSql: '',
        },
        {
            position: tmpPosition++,
            name: 'Created',
            storeType: StoreType.timestampTz,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: `(CURRENT_TIMESTAMP AT TIME ZONE 'UTC')`,
        },
        {
            position: tmpPosition++,
            name: 'Created By',
            storeType: StoreType.text,
            isNullable: false,
            computedColumnSql: '',
            defaultValueSql: 'CURRENT_USER',
        }],
        indexes: [{
            name: '',
            indexType: TableIndexType.PrimaryKey,
            columns: ['Id'],
        },
        {
            name: '',
            indexType: TableIndexType.UniqueConstraint,
            columns: ['Slug'],
        }],
    });

    const onsubmitform = async (e: SubmitEvent) => {
        e.preventDefault();

        const input = {
            databaseName: info.workspace,
            schemaName: info.schema,
            table: unwrap(table),
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
        <Breadcrumb link={{ text: 'New Table', href: combinePaths(createZoneUrl(), 'table-maker') }} />

        <section class='pxy md:w-900px max-w-100% md:mx-auto'>

            <h1>Table Maker</h1>

            <form onsubmit={onsubmitform}>

                <div class='flex gap-1 mb rounded paint-gray-100'>
                    <button class='button paint-primary'>
                        <SaveEditFilledIcon />
                        Save
                    </button>
                    <button type='button' class='button'
                        onclick={e => {
                            table.columns = [{
                                position: tmpPosition++,
                                name: 'Id',
                                storeType: StoreType.uuid,
                                isNullable: false,
                                computedColumnSql: '',
                                defaultValueSql: 'gen_random_uuid()',
                            }];
                            table.indexes = [{
                                name: '',
                                indexType: TableIndexType.PrimaryKey,
                                columns: ['Id'],
                            }];
                        }}>
                        <SaveEditFilledIcon />
                        Clear Form
                    </button>
                </div>

                <div class="pxy b rounded">
                    <TableDesignerWidget table={table} />
                </div>

            </form>

        </section>
    </>);
}