import { Route, Routes } from '@solidjs/router';
import { ProtectedRoute } from './widgets/Protected';

import HomePage from './pages';
import JoinPage from './pages/join';
import LoginPage from './pages/login';
import LogoutPage from './pages/logout';
import NotFoundPage from './pages/not-found';
import AccessDeniedPage from './pages/access-denied';
import OrganizationPage, { createOrganizationRouteData } from './pages/organizations/organization';
import MyPage from './pages/my';
import NewOrganizationPage from './pages/organizations/new-organization';
import OrganizationsPage from './pages/organizations';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="**" component={NotFoundPage} />

      <Route path="/" component={HomePage} />

      <Route path="/access-denied" component={AccessDeniedPage} />
      <Route path="/join" component={JoinPage} />
      <Route path="/login" component={LoginPage} />
      <Route path="/logout" component={LogoutPage} />

      <Route path='' component={ProtectedRoute}>
        <Route path="/my" component={MyPage} />

        <Route path="/organizations">
          <Route path='/' component={OrganizationsPage} />
          <Route path='/new-organization' component={NewOrganizationPage} />
          <Route path="/:organization">
            <Route path='/' component={OrganizationPage} data={createOrganizationRouteData} />
          </Route>
        </Route>

      </Route>
    </Routes>
  )
};