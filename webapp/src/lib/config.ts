import { loadEnvConfig } from "@next/env";

/**
 * Server-side configuration, read once and validated. Everything the app needs from the
 * environment goes through here, so a missing value fails the build (or the boot) with the name of
 * the variable instead of surfacing later as a 401 or an undefined URL.
 *
 * Server only: it pulls in @next/env, which has no business in a client bundle. NEXT_PUBLIC_
 * variables read from client components stay on process.env, where the compiler inlines them.
 */
loadEnvConfig(process.cwd());

function getEnv(name: string) {
  const value = process.env[name];

  if (!value) throw new Error(`Could not find env ${name}`);

  return value;
}

export const authConfig = {
  // Two issuers on purpose: the browser is redirected to the first one, while the token and
  // userinfo calls the server makes go through the second. Under Docker Compose they differ -
  // id.overflow.local resolves through nginx-proxy on the host, keycloak:8080 on the container
  // network - and in development both are the same host.
  kcIssuer: getEnv("AUTH_KEYCLOAK_ISSUER"),
  kcIssuerInternal: getEnv("AUTH_KEYCLOAK_ISSUER_INTERNAL"),
  kcClientId: getEnv("AUTH_KEYCLOAK_ID"),
  kcSecret: getEnv("AUTH_KEYCLOAK_SECRET"),
  secret: getEnv("AUTH_SECRET"),
  url: getEnv("AUTH_URL"),
};

export const apiConfig = {
  // Aspire injects GATEWAY_URL through service discovery when it starts the app; API_URL is what
  // the env files carry for a standalone run.
  baseUrl: process.env.GATEWAY_URL ?? getEnv("API_URL"),
};

export const storageConfig = {
  endpoint: getEnv("MINIO_ENDPOINT"),
  accessKey: getEnv("MINIO_ACCESS_KEY"),
  secretKey: getEnv("MINIO_SECRET_KEY"),
  bucket: getEnv("MINIO_BUCKET"),
  // The URL the browser uses, which is why it is a NEXT_PUBLIC_ variable: the same key is read
  // client-side in lib/util.ts to spot the images that belong to us.
  publicBaseUrl: getEnv("NEXT_PUBLIC_IMAGE_BASE_URL"),
};
