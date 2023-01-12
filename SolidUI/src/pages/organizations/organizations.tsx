import { A } from '@solidjs/router';
import { MainLayout } from '../../partials/MainLayout';
import MyOrganizations from './partials/MyOrganizations';

export default function OrganizationsPage() {
  return (
    <MainLayout>
      <h1>Organizations</h1>

      <div class='flex gap-4'>
        <MyOrganizations />
        <div class='card'>
          <A href='/organizations/new-organization'>New Organization</A>
        </div>
      </div>
    </MainLayout>
  )
}
