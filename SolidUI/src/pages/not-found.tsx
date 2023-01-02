import { setTitle } from '../utils/page';

export default function NotFoundPage() {
  setTitle('Page Not Found');

  return (
    <section class='card md:w-md mx-auto'>
      <h1>Page Not Found</h1>
      <p>The resource or page you were looking for was not found.</p>
    </section>
  );
}
