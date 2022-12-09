import { lazy } from 'solid-js';
import type { RouteDefinition } from '@solidjs/router';

import Home from './pages/home';
// import AboutData from './pages/about.data';

export const routes: RouteDefinition[] = [
  {
    path: '/',
    component: Home,
  },
  // {
  //   path: '/about',
  //   component: lazy(() => import('./pages/about')),
  //   data: AboutData,
  // },

  {
    path: '/create-workspace',
    component: lazy(() => import('./pages/create-workspace'))
  },
  {
    path: '/workspace',
    component: lazy(() => import('./pages/workspace'))
  },
  {
    path: '/workspaces',
    component: lazy(() => import('./pages/workspaces'))
  },

  {
    path: '/login',
    component: lazy(() => import('./pages/login'))
  },
  {
    path: '/logout',
    component: lazy(() => import('./pages/logout'))
  },
  {
    path: '/join',
    component: lazy(() => import('./pages/join'))
  },

  {
    path: '**',
    component: lazy(() => import('./errors/404')),
  },
];
