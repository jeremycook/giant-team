import { PageSettings } from "../partials/Nav";

export const pageSettings: PageSettings = {
  name: 'Error',
  showInNav: () => false,
}

export default function ErrorPage() {

  return (
    <section class='card md:w-md mx-auto'>
      <h1>Error</h1>
      <p>An unexpected error occurred.</p>
    </section>
  );
}
