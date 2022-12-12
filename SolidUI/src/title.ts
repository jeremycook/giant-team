import { createEffect, createSignal } from 'solid-js';

export const [title, titleSetter] = createSignal('Welcome');

/** Define the root context for watching for document title changes. */
export const useTitle = () => createEffect(() => {
    document.title = title();
});