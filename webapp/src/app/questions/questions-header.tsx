"use client";

import { useTagStore } from "@/lib/hooks/use-tag-store";
import { Button } from "@heroui/button";
import { Tab, Tabs } from "@heroui/tabs";
import Link from "next/link";

type Props = {
  tag?: string;
  total: number;
};

export default function QuestionsHeader({ tag, total }: Props) {
  // The page only knows the slug from the query string; the description comes from the store.
  const selectedTag = useTagStore((state) =>
    tag ? state.getTagBySlug(tag) : undefined,
  );

  const tabs = [
    { key: "newest", label: "newest" },
    { key: "active", label: "active" },
    { key: "unanswered", label: "unanswered" },
  ];

  return (
    <div className="flex flex-col w-full border-b gap-4 pb-4">
      <div className="flex justify-between px-6">
        <div className="flex flex-col items-start gap-2">
          <div className="text-3xl font-semibold">
            {tag ? `[${tag}]` : "Newest questions"}
          </div>
          <p className="font-light">{selectedTag?.description}</p>
        </div>
        <Button as={Link} href="/questions/ask" color="secondary">
          ask question
        </Button>
      </div>
      <div className="flex justify-between px-6 items-center">
        <div>
          {total} {total === 1 ? "question" : "questions"}
        </div>
        <div className="flex items-center">
          <Tabs aria-label="question sort options">
            {tabs.map((item) => (
              <Tab key={item.key} title={item.label} />
            ))}
          </Tabs>
        </div>
      </div>
    </div>
  );
}
