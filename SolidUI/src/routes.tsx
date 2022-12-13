import { lazy } from 'solid-js';
import { Route, Routes } from '@solidjs/router';

import Home from './pages/home';
import WorkspacePageData from './pages/workspace/workspace.data';
import TablePageData from './pages/workspace/schemas/tables/table.data';

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/" component={Home}  />

      <Route path="/workspaces">
        <Route path='/' component={lazy(() => import('./pages/workspaces'))} />
        <Route path='/create-workspace' component={lazy(() => import('./pages/workspaces/create-workspace'))} />
      </Route>

      <Route path="/workspace/:workspace" component={lazy(() => import('./pages/workspace/workspace'))} data={WorkspacePageData}>
        <Route path='/' element={<strong>It worked!</strong>} />
        <Route path='/import-data' component={lazy(() => import('./pages/workspace/import-data'))} />
        <Route path='/schemas/:schema/tables/:table' component={lazy(() => import('./pages/workspace/schemas/tables/table'))} data={TablePageData} />
      </Route>

      <Route path="/login" component={lazy(() => import('./pages/login'))} />
      <Route path="/logout" component={lazy(() => import('./pages/logout'))} />
      <Route path="/join" component={lazy(() => import('./pages/join'))} />

      <Route path="**" component={lazy(() => import('./errors/404'))} />
    </Routes>
  )
};

// // {
// //   path: '/login',
// //   component: lazy(() => import('./pages/login'))
// // },
// // {
// //   path: '/logout',
// //   component: lazy(() => import('./pages/logout'))
// // },
// // {
// //   path: '/join',
// //   component: lazy(() => import('./pages/join'))
// // },

// // {
// //   path: '**',
// //   component: lazy(() => import('./errors/404')),
// // },
// export const routes: RouteDefinition[] = [
//   {
//     path: '/',
//     component: Home,
//   },

//   {
//     path: '/workspaces',
//     component: lazy(() => import('./pages/workspaces'))
//   },
//   {
//     path: '/workspaces/create',
//     component: lazy(() => import('./pages/workspaces/create'))
//   },

//   {
//     path: '/workspace/:workspace',
//     component: lazy(() => import('./pages/workspace/workspace')),
//     data: WorkspacePageData,
//   },
//   {
//     path: '/workspace/:workspace/import-data',
//     component: lazy(() => import('./pages/workspace/import-data')),
//   },
//   {
//     path: '/workspace/:workspace/schemas/:schema/tables/:table',
//     component: lazy(() => import('./pages/workspace/schemas/tables/table')),
//     data: TablePageData,
//   },

//   {
//     path: '/login',
//     component: lazy(() => import('./pages/login'))
//   },
//   {
//     path: '/logout',
//     component: lazy(() => import('./pages/logout'))
//   },
//   {
//     path: '/join',
//     component: lazy(() => import('./pages/join'))
//   },

//   {
//     path: '**',
//     component: lazy(() => import('./errors/404')),
//   },
// ];
