import AuthTestButton from "@/app/session/auth-test-button";
import ErrorButtons from "@/app/session/error-buttons";
import { auth } from "@/auth";
import { Snippet } from "@heroui/snippet";

export default async function SessionPage() {
  const session = await auth();

  return (
    <div className="px-6 w-full">
      <div className="text-center">
        <h3 className="text-3xl">Session dashboard</h3>
      </div>
      <div className="flex items-center gap-3 justify-center mt-6">
        <ErrorButtons />
        <AuthTestButton />
      </div>
      {/* The access token is a very long string, so the pre slot is told to wrap it. */}
      <Snippet
        symbol=""
        color="primary"
        classNames={{
          base: "w-full mt-4",
          pre: "text-wrap whitespace-pre-wrap break-all",
        }}
      >
        {JSON.stringify(session, null, 2)}
      </Snippet>
    </div>
  );
}
