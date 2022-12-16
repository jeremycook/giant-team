import { Accessor, createEffect, createSignal, JSX, onCleanup, onMount, ParentProps } from "solid-js";
import { Portal } from "solid-js/web";
import { constrainToViewport } from "../utils/htmlHelpers";
import { DismissIcon, MoveIcon } from "../utils/icons";

export interface DialogProps {
    title: string,
    onDismiss: JSX.EventHandlerUnion<HTMLButtonElement, MouseEvent>
    mount?: Node,
    initialPosition?: Accessor<{ left: number, top: number }>
}

export default function Dialog(props: DialogProps & ParentProps) {
    let ref: HTMLDivElement = null as any;
    let moveRef: HTMLButtonElement = null as any;

    const [mounted, setMounted] = createSignal(0);
    const [left, setLeft] = createSignal(0);
    const [top, setTop] = createSignal(0);

    let mouseDown = false;
    let offsetLeft = 0, offsetTop = 0;
    const onStartMove = function (this: HTMLButtonElement, mouse: MouseEvent): void {
        offsetLeft = ref.offsetLeft - mouse.clientX;
        offsetTop = ref.offsetTop - mouse.clientY;
        mouseDown = true;
    };
    const onmouseup = () => {
        if (mouseDown) {
            // Snap to viewport after release
            const constrained = constrainToViewport({
                left: left(),
                top: top(),
                width: ref.clientWidth,
                height: ref.clientHeight,
            });
            setLeft(constrained.left);
            setTop(constrained.top);
        }
        mouseDown = false;
    };
    const onmousemove = (mouse: MouseEvent) => {
        mouse.preventDefault();
        if (mouseDown) {
            setLeft(mouse.clientX + offsetLeft);
            setTop(mouse.clientY + offsetTop);
        }
    };
    onMount(() => {
        moveRef.addEventListener('mousedown', onStartMove, true);
        document.addEventListener('mouseup', onmouseup, true);
        document.addEventListener('mousemove', onmousemove, true);

        setMounted(m => m + 1);
    });

    createEffect(() => {
        mounted();
        const initialPosition = props.initialPosition?.();

        if (initialPosition) {
            const position = constrainToViewport({
                // Center horizontally
                left: initialPosition.left - (ref.clientWidth / 2),
                top: initialPosition.top,
                width: ref.clientWidth,
                height: ref.clientHeight,
            });

            setLeft(position.left);
            setTop(position.top);
        }
    });

    onCleanup(() => {
        document.removeEventListener('mousemove', onmousemove);
        document.removeEventListener('mouseup', onmouseup);
    });

    return (
        <Portal mount={props.mount ?? document.body}>
            <div ref={ref} class='card p-2 absolute min-w-300px' style={{ left: left() + 'px', top: top() + 'px' }} role='dialog'>
                <div class='flex gap-1 mb'>
                    <button ref={moveRef} class='cursor-grab invisible md:visible' aria-hidden>
                        <MoveIcon />
                    </button>
                    <strong class='text-xl grow text-center'>{props.title}</strong>
                    <button type='button' onclick={props.onDismiss} aria-label="Close Dialog">
                        <DismissIcon />
                    </button>
                </div>
                {props.children}
            </div>
        </Portal>
    )
}