export default function StatusCodePage({ status, statusText, message }: { status: number, statusText: string, message: string }) {
  return (
    <section class='card md:w-md mx-auto'>
      <h1>{status}: {statusText}</h1>
      <p>{message}</p>
    </section>
  );
}
