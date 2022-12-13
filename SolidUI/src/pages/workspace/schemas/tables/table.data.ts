import { createResource } from 'solid-js';
import { postFetchRecords } from '../../../../api/GiantTeam.Data.Api';

export const fetchTable = async (params: any) => {
    const output = await postFetchRecords({
        database: params.workspace,
        schema: params.schema,
        table: params.table,
    });

    return Object.assign(output, { title: params.table });
};

export const TablePageData = ({ params }: { params: any }) => {
    const [table] = createResource(() => {
        console.log('TablePageData', params);
        return ({
            workspace: params.workspace,
            schema: params.schema,
            table: params.table
        })
    }, fetchTable);
    return table;
};

export default TablePageData;