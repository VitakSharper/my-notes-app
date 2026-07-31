import { z } from "zod";

// Zod has no `required` helper: a trimmed string with a minimum of one is the idiom.
const required = (name: string) =>
  z.string().trim().min(1, { message: `${name} is required` });

export const questionSchema = z.object({
  title: required("Title"),
  content: required("Content").min(10, {
    message: "Content should be at least 10 characters",
  }),
  tags: z
    .array(z.string())
    .min(1, { message: "Select at least one tag" })
    .max(5, { message: "No more than five tags can be selected" }),
});

export type QuestionSchema = z.infer<typeof questionSchema>;
