"use server";

import { auth } from "@/auth";
import { fetchClient } from "@/lib/fetch-client";

// Hits the [Authorize] test endpoint: succeeds only once the request carries a valid token.
export async function testAuth() {
  return fetchClient<string>("/test/auth", "GET");
}

/**
 * No try/catch around auth(): reading the cookies during a build throws a DynamicServerError on
 * purpose, which is how Next.js learns the route is dynamic. Catching it and logging it is what
 * filled the build output with "Dynamic server usage" dumps.
 */
export async function getCurrentUser() {
  const session = await auth();

  return session?.user ?? null;
}
