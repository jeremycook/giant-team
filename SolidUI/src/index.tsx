import './directives';
const pages: Record<string, any> = (import.meta as any).glob(['./pages/**/*.tsx', '!./pages/**/partials/*', '!./pages/**/_*'], { eager: true });

import { Dynamic, ErrorBoundary, For, render } from 'solid-js/web';

import './style/reset.css';
import './style/typo.css';
import 'uno.css';
import './style/plugins.css';
import './style/overrides.css';
import { A, go, here, PageInfo } from './partials/Nav';
import { Show, ValidComponent } from 'solid-js';
import { Do } from './widgets/Do';
import { log } from './helpers/logging';
import NotFoundPage from './pages/not-found';
import { createStore } from 'solid-js/store';
import { reveal } from "./helpers/objectHelpers";

function Main() {
    const [siteMap,] = createStore(Object.entries(pages)
        // .filter(([, p]) => p.pageInfo)
        .map(([key, p]) => {
            const route = key.substring('./pages'.length, key.length - '.tsx'.length).replace(/\/index$/i, '/');
            const pageInfo = p.pageInfo as PageInfo | undefined;

            return {
                key,
                route: route,
                name: pageInfo?.name ?? route,
                showInNav: typeof pageInfo === 'undefined' ? (() => false) : (pageInfo.showInNav ?? (() => true)),
                component: p.default as ValidComponent
            }
        }));

    const getPage = () => {
        return siteMap.find(p => p.route === here.pathname);
    }

    return (<>
        <ul>
            <For each={siteMap}>{page =>
                <Show when={page.showInNav()}>
                    <li>
                        <A href={page.route}>{reveal(page.name)}</A>
                    </li>
                </Show>
            }</For>
        </ul>

        <hr />

        <ErrorBoundary fallback={(err, reset) => <Do>{() => {

            log.error('Page component error: {Error}', { Error: err });
            go('/error');
            reset();

        }}</Do>}>{() => {

            const page = getPage();
            if (page) {
                document.title = reveal(page.name);
                return <Dynamic component={page.component} />
            }
            else {
                document.title = 'Not Found';
                return <Dynamic component={NotFoundPage} />
            }

        }}</ErrorBoundary>
    </>)
}

render(
    () => <Main />,
    document.getElementById('root') as HTMLElement,
);
