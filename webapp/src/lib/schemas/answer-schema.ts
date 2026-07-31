import { contentField } from "@/lib/schemas/content-field";
import { z } from "zod";

export const answerSchema = z.object({
  content: contentField,
});

export type AnswerSchemaInput = z.input<typeof answerSchema>;
export type AnswerSchema = z.output<typeof answerSchema>;
