"use server";

import { auth } from "@/auth";
import { fetchClient } from "@/lib/fetch-client";

// Hits the [Authorize] test endpoint: succeeds only once the request carries a valid token.
export async function testAuth() {
  return fetchClient<string>("/test/auth", "GET");
}

export async function getCurrentUser() {
  try {
    const session = await auth();

    if (!session) return null;

    return session.user;
  } catch (error: unknown) {
    console.log(error);
    return null;
  }
}
