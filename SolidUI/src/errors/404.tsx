import { titleSetter } from "../title";

export default function NotFound() {
  titleSetter("404: Not Found");

  return (
    <section class="card md:w-md mx-auto">
      <h1>404: Not Found</h1>
      <p>It's gone 😞</p>
    </section>
  );
}
