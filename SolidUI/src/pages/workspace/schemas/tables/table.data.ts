import { RouteDataFuncArgs } from '@solidjs/router';
import { createResource } from 'solid-js';
import { FetchRecordsInput } from '../../../../api/GiantTeam';
import { postFetchRecords } from '../../../../api/GiantTeam.Data.Api';

export const fetchTable = async (params: FetchRecordsInput) => {
    const output = await postFetchRecords({
        database: params.database,
        schema: params.schema,
        table: params.table,
        columns: params.columns,
        filters: params.filters,
        // orderBy: params.orderBy,
        skip: params.skip,
        take: params.take
    });

    return Object.assign(output, { title: params.table });
};

export const TablePageData = ({ params, location }: RouteDataFuncArgs) => {

    const [table] = createResource(() => {
        return ({
            database: params.workspace,
            schema: params.schema,
            table: params.table,
            orderBy: location.query.orderBy ? [{ column: location.query.orderBy as string }] : [],
        })
    }, fetchTable);
    return table;
};

export default TablePageData;