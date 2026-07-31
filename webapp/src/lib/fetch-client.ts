import { ApiResponse } from "@/lib/types";
import { notFound } from "next/navigation";

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

// RequestInit already has a body, but it only accepts BodyInit (string, FormData, ...).
// We want callers to hand us a plain object and let the client serialise it.
type FetchOptions = Omit<RequestInit, "body"> & { body?: unknown };

/**
 * Thin wrapper around the fetch that Next.js extends, used by the server actions.
 *
 * GATEWAY_URL is injected by the Aspire AppHost (host URL in development, http://gateway:8001
 * under Docker Compose); API_URL from .env.local is the fallback when the app runs standalone.
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

  const apiUrl = process.env.GATEWAY_URL ?? process.env.API_URL;

  if (!apiUrl) throw new Error("Missing API URL");

  const headers: HeadersInit = {
    "Content-Type": "application/json",
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

    if (typeof parsed === "string") message = parsed;
    else if (parsed?.message) message = parsed.message;

    if (!message) message = getFallbackMessage(response.status);

    return { data: null, error: { message, status: response.status } };
  }

  return { data: parsed as T };
}

function getFallbackMessage(status: number) {
  switch (status) {
    case 400:
      return "Bad request. Please check your inputs";
    case 401:
      return "You must be logged in";
    case 403:
      return "You do not have permission to access this resource";
    case 500:
      return "Server error. Please try again later";
    default:
      return "An unexpected error occurred";
  }
}
