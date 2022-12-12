import { lazy } from 'solid-js';
import type { RouteDefinition } from '@solidjs/router';

import Home from './pages/home';
import WorkspaceData from './pages/workspace/workspace.data';

export const routes: RouteDefinition[] = [
  {
    path: '/',
    component: Home,
  },

  {
    path: '/workspaces',
    component: lazy(() => import('./pages/workspaces'))
  },
  {
    path: '/workspaces/create',
    component: lazy(() => import('./pages/workspaces/create'))
  },

  {
    path: '/workspace/:id',
    component: lazy(() => import('./pages/workspace/workspace')),
    data: WorkspaceData,
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
