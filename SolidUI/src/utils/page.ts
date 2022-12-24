import { createEffect, JSX } from 'solid-js';
import { createStore } from 'solid-js/store';

enum AlertType {
    Error,
    Success,
}

export const [page, setPage] = createStore({
    title: [],
    alerts: [] as { visible: boolean, ts: Date, type: AlertType, content: JSX.Element }[],
    showAlerts: false,
});

export const title = () => page.title.join(' • ');

export const setTitle = (title: string) => {
    // setPage('title', [title]);
}

export const pushError = (content: JSX.Element) => {

    const newAlert = {
        visible: true,
        ts: new Date(),
        type: AlertType.Error,
        content: content
    };

    setPage('alerts', a => [newAlert, ...a]);
    setPage('showAlerts', true);

    // // Hide after 5 seconds
    // setTimeout(() => {
    //     const index = page.alerts.findIndex(a => a === newAlert);
    //     setPage('alerts', index, 'visible', false);
    // }, 5000)
}

// createEffect(() => {
//     document.title = title();
// });