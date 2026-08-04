import { getProfiles } from "@/lib/actions/profile-actions";
import { Author, ProfileSummary } from "@/lib/types";

/**
 * The profiles for a set of user ids, keyed by id.
 *
 * Deliberately not a `"use server"` module: every export of one is reachable by a direct POST, and
 * this is server-side plumbing the client has no business calling. Only the thin action it wraps
 * lives there.
 *
 * Never throws and never navigates. fetchClient sends a 404 to the not-found page and a 500 to the
 * error boundary, which is right for a question but wrong for the name beside it: a ProfileService
 * in trouble must not take a question list down. Any failure comes back as an empty map and every
 * author falls back to the name the question already carries.
 */
export async function resolveAuthors(ids: string[]) {
  // The endpoint deduplicates too, but the same user holding several answers on one question
  // should not pad the query string.
  const unique = [...new Set(ids)];

  try {
    const { data } = await getProfiles(unique);

    return new Map(
      (data ?? []).map((profile) => [profile.userId, profile] as const),
    );
  } catch {
    return new Map<string, ProfileSummary>();
  }
}

/** Applies the fallback once, so no component has to. */
export function toAuthor(
  id: string,
  fallbackName: string,
  profiles: Map<string, ProfileSummary>,
): Author {
  const profile = profiles.get(id);

  return {
    id,
    displayName: profile?.displayName ?? fallbackName,
    reputation: profile?.reputation ?? null,
  };
}
