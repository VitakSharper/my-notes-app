"use server";

import { fetchClient } from "@/lib/fetch-client";
import { Tag } from "@/lib/types";

export async function getTags() {
  // Tags barely change and are fetched twice per page load (the store and the tags page),
  // so they go in the Next data cache: force-cache opts in, revalidate expires it after an hour.
  return fetchClient<Tag[]>("/tags", "GET", {
    cache: "force-cache",
    next: { revalidate: 3600 },
  });
}
