import { Tag } from "@/lib/types";
import { create } from "zustand";

type TagStore = {
  tags: Tag[];
  setTags: (tags: Tag[]) => void;
  getTagBySlug: (slug: string) => Tag | undefined;
};

// Global client state: the tags are loaded once in the providers so any client component can
// reach them without prop drilling - the questions header needs the description of a slug it
// only knows by name.
export const useTagStore = create<TagStore>((set, get) => ({
  tags: [],
  setTags: (tags) => set({ tags }),
  getTagBySlug: (slug) => get().tags.find((tag) => tag.slug === slug),
}));
