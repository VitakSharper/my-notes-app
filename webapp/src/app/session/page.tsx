import ErrorButtons from "@/app/session/error-buttons";

export default function SessionPage() {
  return (
    <div className="w-full px-6">
      <h1 className="text-3xl font-semibold">User session</h1>
      <ErrorButtons />
    </div>
  );
}
