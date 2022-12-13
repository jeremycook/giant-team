import { useRouteData } from '@solidjs/router';
import { createEffect, For, Show } from 'solid-js';
import { authorize } from '../../../../session';
import { titleSetter } from '../../../../title';
import TablePageData from './table.data';

export default function WorkspacePage() {
    authorize();
    titleSetter('Workspace');

    const model = useRouteData<() => ReturnType<typeof TablePageData>>();

    const ok = () => model()?.ok == true;
    const message = () => model()?.message || null;
    const data = () => model()?.data;

    createEffect(() => titleSetter(model()?.title ?? 'Table'));

    return (
        <section>

            <Show when={model()}>

                <Show when={message}>
                    <p class={(ok() ? 'text-ok' : 'text-error')} role='alert'>
                        {message}
                    </p>
                </Show>

                <Show when={ok() && data()}>

                    <div>
                        <table>
                            <thead>
                                <tr>
                                    <For each={data()?.columns}>{column =>
                                        <th>{column.name}</th>
                                    }</For>
                                </tr>
                            </thead>
                            <tbody>
                                <For each={data()?.records}>{record =>
                                    <tr>
                                        <For each={data()?.columns}>{(_, i) =>
                                            <td>{record[i()]}</td>}
                                        </For>
                                    </tr>
                                }</For>
                            </tbody>
                        </table>

                    </div>

                </Show>

            </Show>

        </section>
    );
}
