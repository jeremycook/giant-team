import { ParentProps } from "solid-js";
import { SpinnerIcon } from "./Icons";

export function Loading(props: ParentProps) {
    return <>
        <SpinnerIcon class='animate-spin' />
        {props.children ?? 'Loading…'}
    </>
}