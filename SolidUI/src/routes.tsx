import { lazy } from 'solid-js';
import { Route, Routes } from '@solidjs/router';

import HomePage from './pages/home';
import Protected from './widgets/Protected';
import { WorkspacesPageData } from './pages/workspaces/list-workspaces';
import JoinPage from './pages/join';
import LoginPage from './pages/login';
import LogoutPage from './pages/logout';
import NotFoundPage from './pages/not-found';
import AccessDeniedPage from './pages/access-denied';
import { WorkspaceLayoutData } from './pages/workspace/workspace-layout';

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

        <Route path="/workspace/:workspace" component={lazy(() => import('./pages/workspace/workspace-layout'))} data={WorkspaceLayoutData}>

          <Route path='/' component={lazy(() => import('./pages/workspace/workspace'))} />

          <Route path="/zone/:zone" component={lazy(() => import('./pages/workspace/zone/zone-layout'))}>
            <Route path='/' component={lazy(() => import('./pages/workspace/zone/zone'))} />

            <Route path='/import-data' component={lazy(() => import('./pages/workspace/import-data'))} />

            <Route path='/table/:table' component={lazy(() => import('./pages/workspace/zone/table'))} />
            <Route path='/table-designer' component={lazy(() => import('./pages/workspace/zone/table-designer'))} />
            <Route path='/new-table' component={lazy(() => import('./pages/workspace/zone/new-table'))} />

            <Route path='/new-view' component={lazy(() => import('./pages/workspace/zone/new-view'))} />
            <Route path='/view' component={lazy(() => import('./pages/workspace/zone/view'))} />

          </Route>

        </Route>

      </Route>
    </Routes>
  )
};