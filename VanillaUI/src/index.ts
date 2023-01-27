import './helpers/jsx/global';

import './style/reset.css';
import './style/typo.css';
import './style/plugins.css';
import './style/overrides.css';

import Site from './pages/Site';

const root = document.getElementById('root')!;
root.append(Site());