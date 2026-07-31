"use client";

import { testAuth } from "@/lib/actions/auth-actions";
import { handleError, successToast } from "@/lib/util";
import { Button } from "@heroui/button";

export default function AuthTestButton() {
  const onClick = async () => {
    const { data, error } = await testAuth();

    if (error) return handleError(error);
    if (data) successToast(data);
  };

  return (
    <Button color="success" type="button" onPress={onClick}>
      test auth
    </Button>
  );
}
