import { lazy } from 'solid-js';
import type { RouteDefinition } from 'solid-app-router';

import Home from './pages/home';
import AboutData from './pages/about.data';

export const routes: RouteDefinition[] = [
  {
    path: '/',
    component: Home,
  },
  {
    path: '/about',
    component: lazy(() => import('./pages/about')),
    data: AboutData,
  },

  {
    path: '/create-workspace',
    component: lazy(() => import('./pages/create-workspace'))
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
