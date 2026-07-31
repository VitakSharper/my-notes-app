import { stripHtmlTags } from "@/lib/util";
import { z } from "zod";

// Zod has no `required` helper: a trimmed string with a minimum of one is the idiom.
const required = (name: string) =>
  z.string().trim().min(1, { message: `${name} is required` });

// The editor starts out undefined and never returns plain text, so the content field accepts
// undefined, normalises it, then validates the text with the tags stripped out.
const contentField = z
  .union([z.string(), z.undefined()])
  .transform((value) => value ?? "")
  .refine((value) => value.trim().length > 0, {
    message: "Content is required",
  })
  .refine((value) => stripHtmlTags(value).length >= 10, {
    message: "Content should be at least 10 characters",
  });

export const questionSchema = z.object({
  title: required("Title"),
  content: contentField,
  tags: z
    .array(z.string())
    .min(1, { message: "Select at least one tag" })
    .max(5, { message: "No more than five tags can be selected" }),
});

// The transform on content makes the input and output types differ (content?: string in, string
// out), and react-hook-form needs both: the form holds the input shape, handleSubmit hands over
// the output shape.
export type QuestionSchemaInput = z.input<typeof questionSchema>;
export type QuestionSchema = z.output<typeof questionSchema>;
