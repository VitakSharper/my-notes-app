"use server";

import { fetchClient } from "@/lib/fetch-client";
import { Question, SearchQuestion } from "@/lib/types";

export async function getQuestions(tag?: string) {
  const url = tag ? `/questions?tag=${tag}` : "/questions";

  return fetchClient<Question[]>(url, "GET");
}

export async function getQuestionById(id: string) {
  return fetchClient<Question>(`/questions/${id}`, "GET");
}

// Goes to the SearchService (Typesense) through the gateway, not to the QuestionService.
export async function searchQuestions(query: string) {
  return fetchClient<SearchQuestion[]>(
    `/search?query=${encodeURIComponent(query)}`,
    "GET",
  );
}
