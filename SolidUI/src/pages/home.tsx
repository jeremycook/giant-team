import { setTitle, title } from '../title';

export default function HomePage() {
  setTitle('Welcome!');

  return (
    <section class='card md:w-md md:mx-auto'>
      <h1>{title()}</h1>
      <p>
        We'll put something useful here soon!
      </p>
    </section>
  );
}
