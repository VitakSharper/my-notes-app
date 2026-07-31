import { contentField } from "@/lib/schemas/content-field";
import { z } from "zod";

// Zod has no `required` helper: a trimmed string with a minimum of one is the idiom.
const required = (name: string) =>
  z.string().trim().min(1, { message: `${name} is required` });

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
