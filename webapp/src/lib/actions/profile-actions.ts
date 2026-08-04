"use server";

import { fetchClient } from "@/lib/fetch-client";
import { ProfileSummary } from "@/lib/types";

/**
 * The display name and reputation for a set of user ids, in one request.
 *
 * The endpoint is anonymous on the service side on purpose: questions are readable without signing
 * in, so their authors have to resolve for those readers too.
 */
export async function getProfiles(ids: string[]) {
  // The endpoint would answer 200 with an empty array, but there is no point in the round trip.
  if (ids.length === 0) return { data: [] as ProfileSummary[] };

  return fetchClient<ProfileSummary[]>(
    `/profiles/batch?ids=${ids.map(encodeURIComponent).join(",")}`,
    "GET",
  );
}
