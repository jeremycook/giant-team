import { A } from "@solidjs/router";
import { For } from "solid-js";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { link } from "../../../helpers/link";

export function Explorer(props: { organizationId: string, inode: Inode }) {
    return <>
        <p>
            You are here: {props.organizationId}/{props.inode.path}.
        </p>

        <h1>
            {props.inode.name} - {props.inode.path}
        </h1>

        <For each={props.inode.children}>{inode => (<>
            <div class='card'>
                <A href={link.inode(props.organizationId, inode.path)}>{inode.name}</A>
            </div>
        </>)}</For>
    </>
}