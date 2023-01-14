import { ParentProps } from "solid-js";
import { SpinnerIcon } from "../helpers/icons";

export function Loading(props: ParentProps) {
    return <>
        <SpinnerIcon class='animate-spin' />
        {props.children ?? 'Loading…'}
    </>
}