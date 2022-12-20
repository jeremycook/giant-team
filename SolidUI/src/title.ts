import { createEffect, createSignal } from 'solid-js';

export const [title, setTitle] = createSignal('Welcome');

createEffect(() => {
    document.title = title();
});