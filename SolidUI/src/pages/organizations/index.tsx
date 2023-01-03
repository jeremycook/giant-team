import { A, PageSettings } from '../../partials/Nav';
import { isAuthenticated } from '../../utils/session';
import MyOrganizations from './partials/MyOrganizations';

export const pageSettings: PageSettings = {
  name: 'Organizations',
  showInNav: () => isAuthenticated(),
}

export default function OrganizationsPage() {

  return (
    <section class='card md:w-md md:mx-auto'>
      <h1>Organizations</h1>

      <div class='flex gap-4'>
        <MyOrganizations />
        <div class='card'>
          <A href='/organizations/new-organization'>New Organization</A>
        </div>
      </div>
    </section>
  )
}
