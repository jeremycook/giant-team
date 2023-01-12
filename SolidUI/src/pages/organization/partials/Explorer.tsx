import { For } from "solid-js";
import { Node } from "../../../bindings/GiantTeam.Organization.Etc.Models";

export function Explorer(props: { organizationId: string, data: Node }) {
    return <>
        <p>
            You are here: {props.organizationId}/{props.data.path}.
        </p>

        <h1>
            {props.data.name}
        </h1>

        <For each={props.data.children}>{item => (<>
            <div class='card'>
                {item.name}: {item.typeId}
            </div>
        </>)}</For>
    </>
}