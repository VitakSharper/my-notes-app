"use client";

import Link from "next/link";
import React from "react";

type Props = React.ComponentPropsWithoutRef<typeof Link> & {
  ref?: React.Ref<HTMLAnchorElement>;
};

/**
 * A client-side wrapper around next/link.
 *
 * HeroUI components such as Button and Chip are client components. Passing next/link
 * to their `as` prop from a server component fails in Next 16: a function cannot cross
 * the RSC boundary. Passing this wrapper works because it is already client-side.
 */
export default function LinkComponent({ children, ...props }: Props) {
  return <Link {...props}>{children}</Link>;
}
