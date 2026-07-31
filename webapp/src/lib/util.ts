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
