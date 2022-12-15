import { lazy } from 'solid-js';
import { Route, Routes } from '@solidjs/router';

import Home from './pages/home';
import WorkspacePageData from './pages/workspace/workspace.data';
import Protected from './widgets/Protected';
import { WorkspacesPageData } from './pages/workspaces/list-workspaces';

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="**" component={lazy(() => import('./errors/404'))} />

      <Route path="/" component={Home} />

      <Route path="/join" component={lazy(() => import('./pages/join'))} />
      <Route path="/login" component={lazy(() => import('./pages/login'))} />
      <Route path="/logout" component={lazy(() => import('./pages/logout'))} />

      <Route path='' component={Protected}>
        <Route path="/workspaces">
          <Route path='/' component={lazy(() => import('./pages/workspaces/list-workspaces'))} data={WorkspacesPageData} />
          <Route path='/create-workspace' component={lazy(() => import('./pages/workspaces/create-workspace'))} />
        </Route>

        <Route path="/workspace/:workspace" component={lazy(() => import('./pages/workspace/workspace'))} data={WorkspacePageData}>
          <Route path='/' element={<strong>It worked!</strong>} />
          <Route path='/import-data' component={lazy(() => import('./pages/workspace/import-data'))} />
        </Route>

        <Route path='/workspace/table' component={lazy(() => import('./pages/workspace/schemas/tables/table'))} />
      </Route>
    </Routes>
  )
};