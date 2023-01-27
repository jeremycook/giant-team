import Exception from "../Exception";
import { appendChild, h } from "../h";
import { FunctionElement, HTMLAttributes, JSXElement } from "./jsx";

/**
 * Rerender contents when {@param event} occurs.
 * The content should match this signature: `(e?: Event) => JSXElement`.
 * The very first rendering that is not triggered by any events will be passed a custom "jsx_firstrender" event.
 */
export default function On({ event, ...divAttributes }: { event: keyof string | string[] } & HTMLAttributes<HTMLDivElement>, children: ((e: Event) => JSXElement)[]) {
    const ref = h('div', divAttributes);

    function render(e: Event) {
        ref.replaceChildren('');
        appendChild(ref, () => children.map(child => child(e)));
    }
    render(new CustomEvent('jsx_firstrender'));

    // TODO: Remove event listeners when container is removed
    if (Array.isArray(event)) {
        event.forEach(x => document.addEventListener(x, render));
    }
    else if (typeof event === 'string') {
        document.addEventListener(event, render);
    }
    else {
        throw new Exception(On, 'The {event} argument is not supported.', event);
    }

    return ref;
}