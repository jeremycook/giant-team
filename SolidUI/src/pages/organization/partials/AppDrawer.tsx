import { ProcessOperator } from "./ProcessOperatorContext";
import { For, Show } from "solid-js";
import { apps } from "../../../apps";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";

export function AppDrawer(props: {
    inode: Inode;
    processOperator: ProcessOperator;
    onLaunched: (e: MouseEvent & {
        currentTarget: HTMLButtonElement;
        target: Element;
    }) => void;
}) {
    return <>
        <div class='grid grid-cols-3 gap-1 w-300px'>
            <For each={apps}>{app => <>
                <Show when={app.showInAppDrawer ? app.showInAppDrawer(props.inode) : true}>
                    <button type='button' class='card text-center b b-solid'
                        onclick={(e) => {
                            props.processOperator.launch(app, props.inode);
                            props.onLaunched(e);
                        }}>
                        {app.name}
                    </button>
                </Show>
            </>}</For>
        </div>
    </>;
}
