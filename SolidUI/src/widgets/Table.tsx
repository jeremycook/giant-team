import { For } from "solid-js";
import { JSX } from "solid-js/web/types/jsx";

export interface TableData {
    columns: TableColumn[];
    records: any[][];
}

export interface TableColumn {
    name: string;
    dataType: string;
    nullable: boolean;
}

export default function Table({ data, rowLeader }: { data: TableData, rowLeader?: (record: any[]) => JSX.Element }) {
    return (
        <table>
            <thead>
                <tr>
                    {typeof rowLeader === "function" && <th></th>}
                    <For each={data.columns ?? []}>{col =>
                        <th>{col.name}</th>
                    }</For>
                </tr>
            </thead>
            <tbody>
                <For each={data.records ?? []}>{record =>
                    <tr>
                        {typeof rowLeader === "function" && <td>{rowLeader(record)}</td>}
                        <For each={data.columns ?? []}>{(_, rowNumber) =>
                            <td>{record[rowNumber()]}</td>
                        }</For>
                    </tr>
                }</For>
            </tbody>
        </table>
    );
}