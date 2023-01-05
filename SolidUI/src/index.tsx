import './directives';

import { Dynamic, ErrorBoundary, render } from 'solid-js/web';

import './style/reset.css';
import './style/typo.css';
import 'uno.css';
import './style/plugins.css';
import './style/overrides.css';
import { findPage, go, here } from './partials/Nav';
import { Do } from './widgets/Do';
import { log } from './helpers/logging';
import NotFoundPage from './pages/not-found';
import { reveal } from "./helpers/objectHelpers";
import Breadcrumb from './partials/Breadcrumb';
import { Alerts } from './partials/Alerts';

function Main() {
    return (<>
        <Alerts />

        <Breadcrumb />

        <hr />

        <ErrorBoundary fallback={(err, reset) => <Do>{() => {

            log.error('Page component error: {Error}', { Error: err });
            go('/error');
            reset();

        }}</Do>}>{() => {

            const page = findPage(here.pathname);
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
