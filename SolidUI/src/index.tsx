import './directives';

import { render } from 'solid-js/web';
import { Router } from '@solidjs/router';

import './style/reset.css';
import './style/typo.css';
import 'uno.css';
import './style/plugins.css';
import './style/overrides.css';

import { refreshSession, session } from './utils/session';
import App from './App';
import { BreadcrumbProvider } from './utils/nav';

if (session.status === -1) {
  // Refresh the session before first render
  await refreshSession();
}

render(
  () => (
    <BreadcrumbProvider modifyPageTitle={true}>
      <Router>
        <App />
      </Router>
    </BreadcrumbProvider>
  ),
  document.getElementById('root') as HTMLElement,
);
