import { CardLayout } from "../partials/CardLayout";

export default function StatusCodePage({ status, statusText, message }: { status: number, statusText: string, message: string }) {
  return (
    <CardLayout>
      <h1>{status}: {statusText}</h1>
      <p>{message}</p>
    </CardLayout>
  );
}
