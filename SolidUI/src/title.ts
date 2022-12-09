import { createEffect, createSignal } from "solid-js";

export const [title, titleSetter] = createSignal("Welcome");

export const useTitle = () => createEffect(() => {
    document.title = title();
});