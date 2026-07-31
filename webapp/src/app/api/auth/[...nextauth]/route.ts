import { handlers } from "@/auth";

// Auth.js serves its own routes (signin, callback, signout, session...) from this catch-all.
export const { GET, POST } = handlers;
