import { titleSetter } from '../title';

export default function Home() {

  titleSetter("Welcome!");
  return (
    <section class="card md:w-md md:mx-auto">
      <h1>Welcome!</h1>
      <p>
        We'll put something useful here soon!
      </p>
    </section>
  );
}
