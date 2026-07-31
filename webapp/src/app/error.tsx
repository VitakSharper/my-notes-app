"use client";

import { Button } from "@heroui/button";
import { useEffect } from "react";

type Props = {
  error: Error & { digest?: string };
  // Next 16 renamed the course's `reset` prop: unstable_retry re-fetches and re-renders the
  // segment, while `reset` only clears the error state without going back to the server.
  unstable_retry: () => void;
};

export default function GlobalErrorBoundary({ error, unstable_retry }: Props) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="h-full flex items-center justify-center space-y-6">
      <div className="flex flex-col items-center gap-6">
        <h2 className="text-5xl font-bold">Something went wrong</h2>
        <h3 className="text-3xl text-danger-600">{error.message}</h3>
        <Button color="primary" onPress={() => unstable_retry()}>
          Try again
        </Button>
      </div>
    </div>
  );
}
