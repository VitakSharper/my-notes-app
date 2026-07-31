// Module augmentation: widens the Auth.js Session and JWT with the Keycloak tokens.
// The imports look unused but are required - they make TypeScript resolve each module specifier
// in this file, which is what turns the blocks below into augmentations instead of new modules.
/* eslint-disable @typescript-eslint/no-unused-vars */
import NextAuth from "next-auth";
import { JWT } from "next-auth/jwt";
/* eslint-enable @typescript-eslint/no-unused-vars */

declare module "next-auth" {
  interface Session {
    accessToken: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken: string;
    refreshToken: string;
    /** Unix timestamp in seconds, unlike the session `expires` which Auth.js sets to 30 days. */
    accessTokenExpires: number;
    error?: string;
  }
}
