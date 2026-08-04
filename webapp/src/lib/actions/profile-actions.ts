"use server";

import { fetchClient } from "@/lib/fetch-client";
import { ProfileSummary, UserProfile } from "@/lib/types";

/**
 * One profile, for the page the question cards link to.
 *
 * An unknown id answers 404, which fetchClient turns into the not-found page - the right outcome
 * here, unlike in the batch call where a missing profile is only a name that has to fall back.
 */
export async function getProfileById(id: string) {
  return fetchClient<UserProfile>(`/profiles/${id}`, "GET");
}

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
