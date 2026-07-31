// Module augmentation: widens the Auth.js Session and JWT with the Keycloak tokens.
// The imports look unused but are required - they make TypeScript resolve each module specifier
// in this file, which is what turns the blocks below into augmentations instead of new modules.
/* eslint-disable @typescript-eslint/no-unused-vars */
import NextAuth from "next-auth";
import { JWT } from "next-auth/jwt";
/* eslint-enable @typescript-eslint/no-unused-vars */
import { UserProfile } from "@/lib/types";

declare module "next-auth" {
  interface Session {
    accessToken: string;
    // Replaces the default name/email/image user: what the app knows about the user comes from the
    // profile service, not from the token.
    user: UserProfile;
  }

  // The default User is name/email/image; ours is the profile, so every field is redeclared here
  // rather than extended (an empty extending interface is an ESLint error).
  interface User {
    id: string;
    displayName: string;
    description: string | null;
    joinedAt: string;
    reputation: number;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken: string;
    refreshToken: string;
    /** Unix timestamp in seconds, unlike the session `expires` which Auth.js sets to 30 days. */
    accessTokenExpires: number;
    error?: string;
    /** Fetched from the profile service at sign-in, then carried on the JWT. */
    user?: UserProfile;
  }
}
