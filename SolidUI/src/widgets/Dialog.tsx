import { createEffect, createSignal, JSX, onCleanup, onMount, ParentProps } from "solid-js";
import { Portal } from "solid-js/web";
import { constrainToViewport } from "../helpers/htmlHelpers";
import { DismissIcon } from "../partials/Icons";

let globalZIndex = 0;

export enum DialogAnchor {
    topLeft,
    topCenter,
    middleCenter,
}

export interface DialogProps {
    title: string,
    onDismiss: JSX.EventHandlerUnion<HTMLButtonElement, MouseEvent>
    mount?: Node,
    initialPosition?: { x: number, y: number },
    anchor?: DialogAnchor,
}

export default function Dialog(props: DialogProps & ParentProps) {
    let ref: HTMLDivElement = null as any;
    let moveRef: HTMLDivElement = null as any;

    const [mounted, setMounted] = createSignal(0);
    const [left, setLeft] = createSignal(0);
    const [top, setTop] = createSignal(0);
    const [zIndex, setZIndex] = createSignal(++globalZIndex);
    const [grabbing, setGrabbing] = createSignal(false);

    let mouseDown = false;
    let offsetLeft = 0, offsetTop = 0;
    const onmousedown = function (this: HTMLDivElement, mouse: MouseEvent): void {
        offsetLeft = ref.offsetLeft - mouse.clientX;
        offsetTop = ref.offsetTop - mouse.clientY;
        mouseDown = true;
        setGrabbing(true);

        if (zIndex() < globalZIndex) {
            setZIndex(++globalZIndex);
        }
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
            setGrabbing(false);
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
        moveRef.addEventListener('mousedown', onmousedown, true);
        document.addEventListener('mouseup', onmouseup, true);
        document.addEventListener('mousemove', onmousemove, true);

        setMounted(m => m + 1);
    });

    createEffect(() => console.log(zIndex()));

    createEffect(() => {
        mounted();
        const initialPosition = props.initialPosition;

        if (initialPosition) {
            const offsetLeft = props.anchor === DialogAnchor.topCenter
                || props.anchor === DialogAnchor.middleCenter ?
                -ref.clientWidth / 2 :
                0;
            const offsetTop =
                props.anchor === DialogAnchor.middleCenter ?
                    -ref.clientHeight / 2 :
                    0;

            const position = constrainToViewport({
                // Mind the anchor
                left: initialPosition.x + offsetLeft,
                top: initialPosition.y + offsetTop,
                width: ref.clientWidth,
                height: ref.clientHeight,
            });

            setLeft(position.left);
            setTop(position.top);
        }
        else {
            const position = constrainToViewport({
                left: 0,
                top: 0,
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
            <div ref={ref} class='card b b-solid shadow-xl p-0 absolute min-w-300px max-w-100% max-h-100%' classList={{ grabbing: grabbing() }} style={{ left: left() + 'px', top: top() + 'px', "z-index": zIndex() }} role='dialog'>
                <div ref={moveRef} class='rounded-t cursor-grab flex gap-1 mb px-2 py-1 paint-gray-100 color-gray-600'>
                    <strong class='text-xl grow text-center'>{props.title}</strong>
                    <button type='button' onclick={props.onDismiss} aria-label="Close Dialog">
                        <DismissIcon />
                    </button>
                </div>
                <div class='p-2'>
                    {props.children}
                </div>
            </div>
        </Portal>
    )
}