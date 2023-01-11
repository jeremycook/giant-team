import { ParentProps } from 'solid-js';
import { NavBar } from './NavBar';

export function SiteLayout(props: ParentProps) {
  return (
    <>
      <NavBar />
      {props.children}
    </>
  );
};
