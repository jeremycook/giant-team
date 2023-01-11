import { Route, Routes } from '@solidjs/router';
import { ProtectedRoute } from './widgets/Protected';

import AccessDeniedPage from './pages/access-denied';
import HomePage from './pages/home';
import JoinPage from './pages/join';
import LoginPage from './pages/login';
import LogoutPage from './pages/logout';
import MyPage from './pages/my';
import NewOrganizationPage from './pages/organizations/new-organization';
import NotFoundPage from './pages/not-found';
import OrganizationPage, { createOrganizationRouteData } from './pages/organizations/organization';
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
          <Route path="/:organization" data={createOrganizationRouteData}>
            <Route path='/*path' component={OrganizationPage} />
          </Route>
        </Route>

      </Route>
    </Routes>
  )
};