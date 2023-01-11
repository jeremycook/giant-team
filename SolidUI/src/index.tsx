import './directives';

import { render } from 'solid-js/web';

import './style/reset.css';
import './style/typo.css';
import 'uno.css';
import './style/plugins.css';
import './style/overrides.css';
import { Router } from '@solidjs/router';
import { AppRoutes } from './AppRoutes';

render(
    () => (
        <Router>
            <AppRoutes />
        </Router>
    ),
    document.getElementById('root') as HTMLElement,
);
