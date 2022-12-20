import { lazy } from 'solid-js';
import { Route, Routes } from '@solidjs/router';

import HomePage from './pages/home';
import WorkspacePageData from './pages/workspace/workspace.data';
import Protected from './widgets/Protected';
import { WorkspacesPageData } from './pages/workspaces/list-workspaces';
import JoinPage from './pages/join';
import LoginPage from './pages/login';
import LogoutPage from './pages/logout';
import NotFoundPage from './pages/not-found';
import AccessDeniedPage from './pages/access-denied';

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="**" component={NotFoundPage} />

      <Route path="/" component={HomePage} />

      <Route path="/access-denied" component={AccessDeniedPage} />
      <Route path="/join" component={JoinPage} />
      <Route path="/login" component={LoginPage} />
      <Route path="/logout" component={LogoutPage} />

      <Route path='' component={Protected}>
        <Route path="/profile" component={lazy(() => import('./pages/profile'))} />

        <Route path="/workspaces">
          <Route path='/' component={lazy(() => import('./pages/workspaces/list-workspaces'))} data={WorkspacesPageData} />
          <Route path='/create-workspace' component={lazy(() => import('./pages/workspaces/create-workspace'))} />
        </Route>

        <Route path="/workspace/:workspace" component={lazy(() => import('./pages/workspace/workspace'))} data={WorkspacePageData}>
          <Route path='/' element={<strong>It worked!</strong>} />
          <Route path='/import-data' component={lazy(() => import('./pages/workspace/import-data'))} />
        <Route path='/table' component={lazy(() => import('./pages/workspace/schemas/tables/table'))} />
        </Route>

      </Route>
    </Routes>
  )
};