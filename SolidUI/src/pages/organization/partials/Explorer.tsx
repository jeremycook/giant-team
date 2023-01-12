import { For } from "solid-js";
import { Datum } from "../../../bindings/GiantTeam.Organization.Etc.Models";

export function Explorer(props: { organizationId: string, datum: Datum }) {
    return <>
        <p>
            You are here: {props.organizationId}/{props.datum.path}.
        </p>

        <h1>
            {props.datum.name}
        </h1>

        <For each={props.datum.children}>{item => (<>
            <div class='card'>
                {item.name}: {item.typeId}
            </div>
        </>)}</For>
    </>
}