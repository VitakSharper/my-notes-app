"use client";

import { triggerError } from "@/lib/actions/error-actions";
import { handleError } from "@/lib/util";
import { Button } from "@heroui/button";
import { useState, useTransition } from "react";

const codes = [400, 401, 403, 404, 500];

export default function ErrorButtons() {
  // useTransition is what lets an error thrown by a server action reach the error boundary
  // when the call starts in a client component.
  const [pending, startTransition] = useTransition();
  const [target, setTarget] = useState(0);

  const onClick = (code: number) => {
    setTarget(code);

    startTransition(async () => {
      const { error } = await triggerError(code);

      if (error) handleError(error);

      setTarget(0);
    });
  };

  return (
    // The session page owns the layout now, so this only spaces its own buttons.
    <div className="flex gap-3">
      {codes.map((code) => (
        <Button
          key={code}
          color="primary"
          type="button"
          // Comparing against the target keeps the spinner on the button that was pressed.
          isLoading={pending && target === code}
          onPress={() => onClick(code)}
        >
          Test {code}
        </Button>
      ))}
    </div>
  );
}
