import './directives';

import { render } from 'solid-js/web';
import { Router } from '@solidjs/router';

import './style/reset.css';
import './style/vars.css';
import './style/typo.css';
import 'uno.css';
import './style/plugins.css';
import './style/overrides.css';
import './style/theme.css';

import { refreshSession, session } from './utils/session';
import App from './App';

if (session.status === -1) {
  // Refresh the session before first render
  await refreshSession();
}

render(
  () => (
    <Router>
      <App />
    </Router>
  ),
  document.getElementById('root') as HTMLElement,
);
