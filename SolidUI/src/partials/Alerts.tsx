import { For, JSX } from "solid-js"
import { createMutable } from "solid-js/store"
import { DismissIcon } from "../helpers/icons";

const lanes = createMutable<{ type: 'info' | 'warn' | 'error', content: JSX.Element, dismissed: boolean }[]>([]);

// TODO: toast things
export const toast = {
    info: (text: JSX.Element) => {
        lanes.push({ type: 'info', content: text, dismissed: false });
    },
    warning: (text: JSX.Element) => {
        lanes.push({ type: 'warn', content: text, dismissed: false });
    },
    error: (text: JSX.Element) => {
        lanes.push({ type: 'error', content: text, dismissed: false });
    },
}

export function Alerts() {
    return <>
        <For each={lanes.filter(o => !o.dismissed)}>{alert => <>
            <div classList={{
                'text-danger': alert.type === 'error',
                'text-warn': alert.type === 'warn',
                'text-info': alert.type === 'info'
            }}>
                <div class='text-right'>
                    <button type='button' onclick={() => alert.dismissed = true}>
                        <DismissIcon />
                    </button>
                </div>
                {alert.content}
            </div>
        </>}</For>
    </>
}