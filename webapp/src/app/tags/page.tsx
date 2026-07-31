import TagCard from "@/app/tags/tag-card";
import TagsHeader from "@/app/tags/tags-header";
import { getTags } from "@/lib/actions/tag-actions";

export default async function TagsPage() {
  const { data: tags, error } = await getTags();

  if (error) throw new Error(error.message);

  return (
    <div className="w-full p-6">
      <TagsHeader />
      <div className="grid grid-cols-3 gap-4">
        {tags?.map((tag) => (
          <TagCard tag={tag} key={tag.id} />
        ))}
      </div>
    </div>
  );
}
