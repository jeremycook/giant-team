import { createEffect, ParentProps } from 'solid-js';

export function Body(props: { class?: string } & ParentProps) {
    createEffect(() => {
        if (props.class) {
            document.body.className = props.class;
        }
    })
    return <>
        {props.children}
    </>
};
