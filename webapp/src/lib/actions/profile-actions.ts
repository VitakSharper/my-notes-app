"use server";

import { unstable_update } from "@/auth";
import { fetchClient } from "@/lib/fetch-client";
import { ProfileSchema } from "@/lib/schemas/profile-schema";
import { ProfileSummary, UserProfile } from "@/lib/types";
import { revalidatePath } from "next/cache";

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
 * Saves the signed-in user's own profile. There is no id to pass: the service reads the caller from
 * the token, so no one can write someone else's row by changing a URL.
 */
export async function updateProfile(data: ProfileSchema) {
  const result = await fetchClient<UserProfile>("/profiles/me", "PUT", {
    body: data,
  });

  if (result.error || !result.data) return result;

  // The session holds the profile as it was at sign-in, so the nav would keep the old display name
  // without this.
  await unstable_update({ user: result.data });

  // Both places a display name is printed from a cached render.
  revalidatePath(`/profiles/${result.data.id}`);
  revalidatePath("/questions");

  return result;
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
