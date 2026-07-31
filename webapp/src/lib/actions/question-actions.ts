"use server";

import { fetchClient } from "@/lib/fetch-client";
import { AnswerSchema } from "@/lib/schemas/answer-schema";
import { QuestionSchema } from "@/lib/schemas/question-schema";
import { Answer, Question, SearchQuestion } from "@/lib/types";

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
  return fetchClient<Answer>(`/questions/${questionId}/answers`, "POST", {
    body: data,
  });
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
