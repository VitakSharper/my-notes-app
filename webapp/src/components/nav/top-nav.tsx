import { Square3Stack3DIcon } from "@heroicons/react/24/solid";
import Link from "next/link";
import LoginButton from "@/components/nav/login-button";
import RegisterButton from "@/components/nav/register-button";
import SearchInput from "@/components/nav/search-input";
import ThemeToggle from "@/components/nav/theme-toggle";
import UserMenu from "@/components/nav/user-menu";
import { getCurrentUser } from "@/lib/actions/auth-actions";

export default async function TopNav() {
  const user = await getCurrentUser();

  return (
    <header className="p-2 w-full fixed top-0 z-50 border-b bg-white dark:bg-black">
      <div className="flex px-10 m-auto">
        <div className="flex items-center gap-6">
          <Link href="/" className="flex items-center gap-3 max-h-16">
            <Square3Stack3DIcon className="size-10 text-secondary" />
            <h3 className="text-xl font-semibold uppercase">Cairn</h3>
          </Link>
          <nav className="flex gap-3 my-2 text-md text-neutral-500">
            <Link href="/">About</Link>
            <Link href="/">Products</Link>
            <Link href="/">Contact</Link>
          </nav>
          <SearchInput />
        </div>
        <div className="flex basis-1/4 shrink-0 justify-end gap-3 items-center">
          <ThemeToggle />
          {user ? (
            <UserMenu user={user} />
          ) : (
            <>
              <LoginButton />
              <RegisterButton />
            </>
          )}
        </div>
      </div>
    </header>
  );
}
