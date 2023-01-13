import { A } from "@solidjs/router";
import { For } from "solid-js";
import { Datum } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { link } from "../../../helpers/link";

export function Explorer(props: { organizationId: string, datum: Datum }) {
    return <>
        <p>
            You are here: {props.organizationId}/{props.datum.path}.
        </p>

        <h1>
            {props.datum.name} - {props.datum.path}
        </h1>

        <For each={props.datum.children}>{datum => (<>
            <div class='card'>
                <A href={link.datum(props.organizationId, datum.path)}>{datum.name}</A>
            </div>
        </>)}</For>
    </>
}