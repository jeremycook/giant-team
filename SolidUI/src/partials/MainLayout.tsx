import { ParentProps } from 'solid-js';
import { SiteLayout } from './SiteLayout';

export function MainLayout(props: ParentProps) {
  return (
    <SiteLayout>
      <main role='main' class='mxy'>
        {props.children}
      </main>
    </SiteLayout>
  );
};
