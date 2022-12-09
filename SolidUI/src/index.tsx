import { render } from 'solid-js/web';
import { Router } from '@solidjs/router';

import './style/reset.css';
import './style/vars.css';
import 'uno.css';
import './style/typo.css';
import './style/plugins.css';
import './style/theme.css';
// import "./index.css";
import App from './App';

render(
  () => (
    <Router>
      <App />
    </Router>
  ),
  document.getElementById('root') as HTMLElement,
);
