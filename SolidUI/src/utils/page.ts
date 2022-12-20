import { createEffect } from 'solid-js';
import { createStore } from 'solid-js/store';

export const [page, setPage] = createStore({
    title: 'Welcome'
});

export const title = () => page.title;

export const setTitle = (title: string) => {
    setPage('title', title);
}

createEffect(() => {
    document.title = page.title;
});