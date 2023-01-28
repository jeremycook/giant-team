import Exception from "../Exception";
import { appendChild, h } from "../h";
import { FunctionElement, HTMLAttributes, JSXElement } from "./jsx";

/**
 * Rerender contents when {@param event} occurs.
 * The content should match the signature: `(e?: Event) => JSXElement`.
 * The very first rendering that is not triggered by any events will be passed a custom "jsx_firstrender" event.
 */
export default function On({ event, events, ...divAttributes }: {
    event?: string | { type: string, element: Element | Window };
    events?: (string | { type: string, element: Element | Window })[];
} & HTMLAttributes<HTMLDivElement>, children: ((e: Event) => JSXElement)[]) {
    const ref = h('div', divAttributes);
    const fragment = document.createDocumentFragment();

    function render(e: Event) {
        appendChild(fragment, () => children.map(child => child(e)));
        ref.replaceChildren(fragment);
    }
    render(new CustomEvent('jsx_firstrender'));

    // TODO: Remove event listeners when container is removed

    events ??= [];
    if (event) {
        events.push(event);
    }

    if (events.length <= 0) throw new Exception(On, 'At least one event type must be provided.');

    for (const ev of events) {
        if (typeof ev === 'string') {
            document.addEventListener(ev, render);
        }
        else if (typeof ev === 'object') {
            ev.element.addEventListener(ev.type, render);
        }
        else {
            throw new Exception(On, 'The {event} argument is not supported.', ev);
        }
    }

    return ref;
}