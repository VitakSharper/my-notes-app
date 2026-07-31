import { stripHtmlTags } from "@/lib/util";
import { z } from "zod";

/**
 * Validation for a rich text editor field, shared by the question and the answer schemas: the
 * editor starts out undefined and never returns plain text, so the value is normalised first and
 * the length is measured with the tags stripped out.
 */
export const contentField = z
  .union([z.string(), z.undefined()])
  .transform((value) => value ?? "")
  .refine((value) => value.trim().length > 0, {
    message: "Content is required",
  })
  .refine((value) => stripHtmlTags(value).length >= 10, {
    message: "Content should be at least 10 characters",
  });
