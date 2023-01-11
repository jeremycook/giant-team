import { ParentProps } from 'solid-js';
import { SiteLayout } from './SiteLayout';

export function CardLayout(props: ParentProps) {
  return (
    <SiteLayout>
      <main class='card md:w-md md:mx-auto mxy'>
        {props.children}
      </main>
    </SiteLayout>
  );
};
