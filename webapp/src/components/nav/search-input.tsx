"use client";

import { searchQuestions } from "@/lib/actions/question-actions";
import { SearchQuestion } from "@/lib/types";
import { handleError } from "@/lib/util";
import { MagnifyingGlassIcon } from "@heroicons/react/24/solid";
import { Input } from "@heroui/input";
import { Listbox, ListboxItem } from "@heroui/listbox";
import { useEffect, useState } from "react";

export default function SearchInput() {
  // A controlled input: React owns the value, which is what lets us debounce it.
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState<SearchQuestion[] | null>(null);
  const [showDropdown, setShowDropdown] = useState(false);

  useEffect(() => {
    if (!query) return;

    const timeout = setTimeout(async () => {
      setLoading(true);

      const { data: questions, error } = await searchQuestions(query);

      setLoading(false);

      if (error) return handleError(error);

      setResults(questions);
      setShowDropdown(true);
    }, 300);

    // Every keystroke re-runs the effect, and the cleanup cancels the pending timer: the
    // request only fires 300ms after the user stops typing.
    return () => clearTimeout(timeout);
  }, [query]);

  const onChange = (value: string) => {
    setQuery(value);

    if (!value) {
      setResults(null);
      setShowDropdown(false);
    }
  };

  // The search box never unmounts, so a selection has to clear it explicitly.
  const onAction = () => {
    setQuery("");
    setResults(null);
    setShowDropdown(false);
  };

  return (
    <div className="flex flex-col w-full relative ml-6">
      <Input
        startContent={<MagnifyingGlassIcon className="size-6" />}
        type="search"
        placeholder="Search"
        value={query}
        onChange={(e) => onChange(e.target.value)}
      />
      {showDropdown && results && (
        // The course sets w-[50%], which works because its wrapper spans the nav; ours is only
        // as wide as the input, so the dropdown gets an explicit width instead.
        <div className="absolute top-full mt-1 z-50 bg-white dark:bg-default-50 shadow-lg border-2 border-default-500 rounded-md w-[30rem] max-w-[70vw]">
          {!loading && results.length === 0 && (
            <span className="p-3 block">No results</span>
          )}
          {!loading && (
            <Listbox
              aria-label="search results"
              onAction={onAction}
              items={results}
              className="flex flex-col overflow-y-auto"
            >
              {(question) => (
                <ListboxItem
                  key={question.id}
                  href={`/questions/${question.id}`}
                  startContent={
                    <div className="flex flex-col h-14 min-w-14 justify-center items-center border border-success rounded-md">
                      <span>{question.answerCount}</span>
                      <span className="text-xs">answers</span>
                    </div>
                  }
                >
                  <div>
                    <div className="font-semibold">{question.title}</div>
                    <div
                      className="text-xs opacity-60 line-clamp-2"
                      dangerouslySetInnerHTML={{ __html: question.content }}
                    />
                  </div>
                </ListboxItem>
              )}
            </Listbox>
          )}
        </div>
      )}
    </div>
  );
}
