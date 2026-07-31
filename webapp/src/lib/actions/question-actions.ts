"use server";

import { fetchClient } from "@/lib/fetch-client";
import { AnswerSchema } from "@/lib/schemas/answer-schema";
import { QuestionSchema } from "@/lib/schemas/question-schema";
import { Answer, Question, SearchQuestion } from "@/lib/types";
import { revalidatePath } from "next/cache";

export async function getQuestions(tag?: string) {
  const url = tag ? `/questions?tag=${tag}` : "/questions";

  return fetchClient<Question[]>(url, "GET");
}

export async function getQuestionById(id: string) {
  return fetchClient<Question>(`/questions/${id}`, "GET");
}

export async function postQuestion(question: QuestionSchema) {
  return fetchClient<Question>("/questions", "POST", { body: question });
}

// The id travels separately: the schema describes the form fields, and has no id of its own.
export async function updateQuestion(question: QuestionSchema, id: string) {
  return fetchClient<Question>(`/questions/${id}`, "PUT", { body: question });
}

export async function postAnswer(data: AnswerSchema, questionId: string) {
  const result = await fetchClient<Answer>(
    `/questions/${questionId}/answers`,
    "POST",
    { body: data },
  );

  // The user stays on the question page, and nothing re-renders it on its own: revalidatePath
  // tells Next.js the data behind that path is stale, so the new answer appears without a
  // refresh. Next 16 also has refresh(), which only re-renders the current page - the path is
  // used here because it is the page the answer belongs to, whichever page we were on.
  if (!result.error) revalidatePath(`/questions/${questionId}`);

  return result;
}

export async function deleteQuestion(id: string) {
  return fetchClient(`/questions/${id}`, "DELETE");
}

// Goes to the SearchService (Typesense) through the gateway, not to the QuestionService.
export async function searchQuestions(query: string) {
  return fetchClient<SearchQuestion[]>(
    `/search?query=${encodeURIComponent(query)}`,
    "GET",
  );
}
