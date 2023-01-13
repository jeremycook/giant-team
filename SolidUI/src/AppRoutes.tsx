import { Route, Routes } from '@solidjs/router';
import { ProtectedRoute } from './widgets/ProtectedRoute';

import AccessDeniedPage from './pages/access-denied';
import HomePage from './pages/home';
import JoinPage from './pages/join';
import LoginPage from './pages/login';
import LogoutPage from './pages/logout';
import NewOrganizationPage from './pages/organizations/new-organization';
import NotFoundPage from './pages/not-found';
import MyPage from './pages/my/my';
import OrganizationPage, { createOrganizationRouteData } from './pages/organization/organization';
import OrganizationsPage from './pages/organizations/organizations';
import ExplorerPage, { createInodeRouteData } from './pages/organization/explorer';

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
        </Route>

        <Route path="/o/:organization" data={createOrganizationRouteData}>
          <Route path='/' component={OrganizationPage} />
          <Route path='/*path' component={ExplorerPage} data={createInodeRouteData} />
        </Route>

      </Route>
    </Routes>
  )
};