"use client";

import { Button } from "@heroui/button";
// Deliberately the client-side signIn: the browser has to take part in the redirect flow, so
// the server-side helper exported from @/auth is not what we want here.
import { signIn } from "next-auth/react";

export default function LoginButton() {
  return (
    <Button
      type="button"
      color="secondary"
      variant="bordered"
      // prompt=login forces Keycloak to ask again instead of silently reusing its SSO session,
      // which is what makes switching user possible. It belongs in the third argument: extra
      // keys in the options object go into the POST body and never reach the provider.
      onPress={() =>
        signIn("keycloak", { redirectTo: "/questions" }, { prompt: "login" })
      }
    >
      Login
    </Button>
  );
}
