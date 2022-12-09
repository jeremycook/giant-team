import { For } from "solid-js";
import { JSX } from "solid-js/web/types/jsx";
import { FetchRecordsOutput } from "../api/GiantTeam";

export default function Table({ data, rowLeader }: { data: FetchRecordsOutput, rowLeader?: (record: any[]) => JSX.Element }) {
    return (
        <table>
            <thead>
                <tr>
                    <For each={data.columns ?? []}>{(col) =>
                        <th>{col.name}</th>
                    }</For>
                </tr>
            </thead>
            <tbody>
                <For each={data.records ?? []}>{(record: any[]) =>
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