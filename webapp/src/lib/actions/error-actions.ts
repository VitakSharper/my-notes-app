"use server";

import { fetchClient } from "@/lib/fetch-client";

// Hits the QuestionService test endpoint, which returns whichever status code we ask for.
export async function triggerError(code: number) {
  return fetchClient(`/questions/errors?code=${code}`, "GET");
}
