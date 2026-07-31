import { ApiError } from "@/lib/types";
import { addToast } from "@heroui/toast";
import {
  differenceInCalendarDays,
  differenceInCalendarMonths,
  differenceInCalendarWeeks,
  formatDistanceToNow,
  isToday,
  isYesterday,
} from "date-fns";

export function errorToast(error: ApiError) {
  return addToast({
    color: "danger",
    title: error.status?.toString() ?? "Error!",
    description: error.message ?? "Something went wrong",
  });
}

export function successToast(message: string, title?: string) {
  return addToast({
    color: "success",
    title: title ?? "Success",
    description: message,
  });
}

/**
 * Default handling for an error a client component gets back from a server action: a toast,
 * except for a server error, which belongs on the error boundary.
 */
export function handleError(error: ApiError) {
  if (error.status === 500) throw new Error(error.message);

  return errorToast(error);
}

const imageBaseUrl =
  process.env.NEXT_PUBLIC_IMAGE_BASE_URL ?? "http://localhost:9000/overflow";

/**
 * The keys of the images currently embedded in a piece of editor HTML, so removing an image from
 * the editor can remove it from storage too.
 *
 * The course parses Cloudinary URLs (folder, version and upload segments) with a long regex; a
 * MinIO URL is just the bucket base plus the key. Matching only our own base is also safer: an
 * image pasted from elsewhere cannot produce a delete call.
 */
export function extractPublicIdsFromHtml(html: string) {
  const publicIds: string[] = [];
  const imageTags = /<img[^>]+src="([^">]+)"/g;

  let match: RegExpExecArray | null;

  while ((match = imageTags.exec(html)) !== null) {
    const url = match[1];

    if (url.startsWith(`${imageBaseUrl}/`)) {
      publicIds.push(url.slice(imageBaseUrl.length + 1));
    }
  }

  return publicIds;
}

/**
 * The rich text editor hands back HTML, so a length check has to count the text only: an empty
 * editor still produces "<p></p>".
 *
 * Block boundaries become a space, otherwise the text of two blocks runs together
 * ("process.envpassing it") when the result is displayed as a preview. Inline tags are removed
 * without a space, so a <code> span keeps the punctuation that follows it attached.
 */
export function stripHtmlTags(html: string) {
  return html
    .replace(/<\/(p|div|li|ul|ol|h[1-6]|blockquote|pre|tr|td)>|<br\s*\/?>/gi, " ")
    .replace(/<[^>]*>/g, "")
    .replace(/\s+/g, " ")
    .trim();
}

/** Coarse "today / yesterday / 3 days ago" used on the question detail header. */
export function fuzzyTimeAgo(date: string | Date) {
  const now = new Date();

  if (isToday(date)) return "today";
  if (isYesterday(date)) return "yesterday";

  const days = differenceInCalendarDays(now, date);
  if (days < 7) return `${days} day${days > 1 ? "s" : ""} ago`;

  const weeks = differenceInCalendarWeeks(now, date);
  if (weeks < 4) return `${weeks} week${weeks > 1 ? "s" : ""} ago`;

  const months = differenceInCalendarMonths(now, date);
  return `${months} month${months > 1 ? "s" : ""} ago`;
}

/** Finer "about 3 hours ago" used on the cards and footers. */
export function timeAgo(date: string | Date) {
  return formatDistanceToNow(date, { addSuffix: true });
}
