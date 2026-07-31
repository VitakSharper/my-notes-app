import { authConfig } from "@/lib/config";
import NextAuth from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    // The provider needs no credentials: Auth.js reads AUTH_KEYCLOAK_ID, AUTH_KEYCLOAK_SECRET
    // and AUTH_KEYCLOAK_ISSUER from the environment by convention - importing authConfig is what
    // makes their absence a startup error rather than a puzzling redirect. offline_access is asked
    // for so the refresh token outlives the Keycloak SSO session.
    Keycloak({
      authorization: {
        params: { scope: "openid profile email offline_access" },
      },
    }),
  ],
  callbacks: {
    async jwt({ token, account, profile }) {
      const now = Math.floor(Date.now() / 1000);

      // token.sub is not the Keycloak user id; the id we need to compare against a question's
      // askerId only ever arrives on the profile, which is populated at sign-in.
      if (profile?.sub) {
        token.sub = profile.sub;
      }

      // `account` is only populated on the sign-in call - the one and only moment Keycloak hands
      // us the tokens - so they have to be copied onto the JWT to survive later calls.
      if (account?.access_token && account.refresh_token) {
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.accessTokenExpires = now + account.expires_in!;
        token.error = undefined;

        return token;
      }

      // Still valid: nothing to do.
      if (token.accessTokenExpires && now < token.accessTokenExpires) {
        return token;
      }

      // Expired: trade the refresh token for a new pair. Auth.js has no built-in rotation yet.
      try {
        const response = await fetch(
          `${authConfig.kcIssuer}/protocol/openid-connect/token`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/x-www-form-urlencoded",
            },
            body: new URLSearchParams({
              grant_type: "refresh_token",
              client_id: authConfig.kcClientId,
              client_secret: authConfig.kcSecret,
              refresh_token: token.refreshToken,
            }),
          },
        );

        const refreshed = await response.json();

        if (!response.ok) {
          console.log("Failed to refresh token", refreshed);
          token.error = "RefreshAccessTokenError";

          return token;
        }

        token.accessToken = refreshed.access_token;
        token.refreshToken = refreshed.refresh_token;
        token.accessTokenExpires = now + refreshed.expires_in;
      } catch (error) {
        console.log("Failed to refresh token", error);
        token.error = "RefreshAccessTokenError";
      }

      return token;
    },
    // Auth.js exposes only name/email/image by default; the session is widened with the token
    // so server actions can send it as a bearer token (see lib/types/next-auth.d.ts).
    async session({ session, token }) {
      if (token.accessToken) {
        session.accessToken = token.accessToken;
      }

      if (token.sub) {
        session.user.id = token.sub;
      }

      if (token.accessTokenExpires) {
        // The default `expires` is a meaningless 30 days. session.expires is an ISO string, so
        // this stays type-safe instead of the double cast the course resorts to.
        session.expires = new Date(
          token.accessTokenExpires * 1000,
        ).toISOString() as typeof session.expires;
      }

      return session;
    },
  },
});
