import type { Metadata } from "next";
import "./globals.css";
import Providers from "@/components/providers";
import TopNav from "@/components/nav/top-nav";
import SideMenu from "@/components/side-menu";

export const metadata: Metadata = {
  title: "Cairn",
  description:
    "Ask a question, leave the answer behind as a marker for whoever comes next.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    // next-themes rewrites the class on this tag on the client, which is a legitimate
    // server/client mismatch. Suppression is scoped to <html> only.
    <html lang="en" className="h-full antialiased" suppressHydrationWarning>
      {/* text-foreground is a HeroUI semantic token: it follows the active theme, so the
          body text stays readable in dark mode. Without it only the backgrounds switch. */}
      <body className="flex flex-col bg-stone-200 dark:bg-default-50 text-foreground h-full">
        <Providers>
          <TopNav />
          <div className="flex grow overflow-auto">
            <aside className="basis-1/6 shrink-0 border-r border-neutral-500 pt-20 sticky top-0 px-6">
              <SideMenu />
            </aside>
            <main className="flex-1 pt-20 h-full">{children}</main>
            <aside className="basis-1/4 shrink-0 px-6 pt-20 bg-stone-300 dark:bg-default-100 sticky top-0">
              right content
            </aside>
          </div>
        </Providers>
      </body>
    </html>
  );
}
