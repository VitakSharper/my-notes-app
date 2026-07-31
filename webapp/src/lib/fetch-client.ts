import { auth } from "@/auth";
import { apiConfig } from "@/lib/config";
import { ApiResponse } from "@/lib/types";
import { notFound } from "next/navigation";

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

// RequestInit already has a body, but it only accepts BodyInit (string, FormData, ...).
// We want callers to hand us a plain object and let the client serialise it.
type FetchOptions = Omit<RequestInit, "body"> & { body?: unknown };

/**
 * Thin wrapper around the fetch that Next.js extends, used by the server actions.
 *
 * The base URL comes from lib/config: GATEWAY_URL when the AppHost injects it, API_URL from the
 * env files otherwise.
 *
 * Errors are returned rather than thrown, except for the two cases where a page transition is
 * the only sensible outcome: 404 goes to the not-found page and 500 to the error boundary.
 */
export async function fetchClient<T>(
  url: string,
  method: HttpMethod,
  options: FetchOptions = {},
): Promise<ApiResponse<T>> {
  const { body, ...rest } = options;

  const apiUrl = apiConfig.baseUrl;

  const session = await auth();

  const headers: HeadersInit = {
    "Content-Type": "application/json",
    // Anonymous requests stay anonymous: the header is only added when a session carries a token.
    ...(session?.accessToken
      ? { Authorization: `Bearer ${session.accessToken}` }
      : {}),
    ...(rest.headers ?? {}),
  };

  const response = await fetch(apiUrl + url, {
    // rest goes first so the method, headers and body we just built always win.
    ...rest,
    method,
    headers,
    ...(body ? { body: JSON.stringify(body) } : {}),
  });

  // The API answers with text/plain for a string body and application/problem+json for a
  // ProblemDetails, so the content type decides how the body is read.
  const contentType = response.headers.get("Content-Type");

  const isJson =
    contentType?.includes("application/json") ||
    contentType?.includes("application/problem+json");

  const parsed = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    if (response.status === 404) notFound();

    if (response.status === 500) {
      throw new Error("Server error. Please try again later");
    }

    let message = "";

    if (response.status === 401) {
      // Neither Next.js nor the Aspire traces say why a token was rejected; Keycloak does, in
      // the WWW-Authenticate header, so the real reason is lifted out of it.
      const authHeader = response.headers.get("WWW-Authenticate");

      if (authHeader?.includes("error_description")) {
        const match = authHeader.match(/error_description="(.+?)"/);
        if (match) message = match[1];
      } else {
        message = "You must be logged in to do that";
      }
    }

    if (!message) {
      if (typeof parsed === "string") message = parsed;
      else if (parsed?.message) message = parsed.message;
      else message = getFallbackMessage(response.status);
    }

    return { data: null, error: { message, status: response.status } };
  }

  return { data: parsed as T };
}

function getFallbackMessage(status: number) {
  switch (status) {
    case 400:
      return "Bad request. Please check your inputs";
    case 403:
      return "You do not have permission to access this resource";
    case 500:
      return "Server error. Please try again later";
    default:
      return "An unexpected error occurred";
  }
}
