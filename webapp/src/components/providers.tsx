"use client";

import { getTags } from "@/lib/actions/tag-actions";
import { useTagStore } from "@/lib/hooks/use-tag-store";
import { HeroUIProvider } from "@heroui/react";
import { ToastProvider } from "@heroui/toast";
import { useRouter } from "next/navigation";
import { ThemeProvider } from "next-themes";
import React, { useEffect } from "react";

export default function Providers({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const setTags = useTagStore((state) => state.setTags);

  // The providers are the first client component to mount, so this is where the tag store is
  // seeded from the server. The fetch itself is cached, so this costs nothing after the first hit.
  useEffect(() => {
    const loadTags = async () => {
      const { data: tags } = await getTags();

      if (tags) setTags(tags);
    };

    void loadTags();
  }, [setTags]);

  return (
    <HeroUIProvider navigate={router.push} className="h-full flex flex-col">
      <ToastProvider />
      <ThemeProvider attribute="class" defaultTheme="light">
        {children}
      </ThemeProvider>
    </HeroUIProvider>
  );
}
