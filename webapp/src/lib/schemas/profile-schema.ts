import { z } from "zod";

// The lengths mirror UpdateProfileDto on the service, which mirrors the entity: the form should
// refuse what a 400 would refuse anyway.
export const profileSchema = z.object({
  displayName: z
    .string()
    .trim()
    .min(1, { message: "Display name is required" })
    .max(200, { message: "Display name cannot be longer than 200 characters" }),
  // Blank comes back as null rather than "", so clearing the field restores the placeholder the
  // profile page shows instead of leaving an empty About behind.
  description: z
    .string()
    .trim()
    .max(1000, { message: "Description cannot be longer than 1000 characters" })
    .transform((value) => (value === "" ? null : value)),
});

// The transform makes the input and output types differ (description: string in, string | null out),
// and react-hook-form needs both: the form holds the input shape, handleSubmit hands over the output.
export type ProfileSchemaInput = z.input<typeof profileSchema>;
export type ProfileSchema = z.output<typeof profileSchema>;
