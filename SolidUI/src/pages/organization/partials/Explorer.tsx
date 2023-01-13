import { A } from "@solidjs/router";
import { For } from "solid-js";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { hrefOf } from "../../../helpers/links";

export function Explorer(props: { organizationId: string, inode: Inode }) {
    return <>
        <p>
            You are here: {props.organizationId}/{props.inode.path}.
        </p>

        <h1>
            {props.inode.name}
        </h1>

        <div class='flex flex-wrap gap-4'>
            <For each={props.inode.children}>{inode => (<>
                <div class='card'>
                    <A href={hrefOf.inode(props.organizationId, inode.path)}>{inode.name}</A>
                </div>
            </>)}</For>
        </div>
    </>
}