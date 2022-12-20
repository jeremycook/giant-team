import { setTitle, title } from '../utils/title';

export default function NotFoundPage() {
  setTitle('Page Not Found');

  return (
    <section class='card md:w-md mx-auto'>
      <h1>{title()}</h1>
      <p>The resource or page you were looking for was not found.</p>
    </section>
  );
}
