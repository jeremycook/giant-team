import { lazy } from 'solid-js';
import { Route, Routes } from '@solidjs/router';

import HomePage from './pages';
import Protected from './widgets/Protected';
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
        <Route path="/profile" component={lazy(() => import('./pages/my'))} />

        <Route path="/workspaces">
          {/* <Route path='/' component={lazy(() => import('./pages/organizations'))} data={WorkspacesPageData} /> */}
          <Route path='/new-organization' component={lazy(() => import('./pages/organizations/new-organization'))} />
        </Route>

      </Route>
    </Routes>
  )
};