import { JSX, Show } from "solid-js";

/** Renders children by passing in the value of when if it is not undefined. */
export function ShowItem<T>(props: { when: T; fallback?: JSX.Element, children: (item: NonNullable<T>) => JSX.Element; }) {
    return <Show keyed={true} when={props.when} fallback={props.fallback}>
        {props.children}
    </Show>;
}
